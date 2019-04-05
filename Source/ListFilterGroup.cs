﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace List_Everything
{
	[StaticConstructorOnStartup]
	static class TexButton
	{
		public static readonly Texture2D Reveal = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reveal", true);
		public static readonly Texture2D Collapse = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Collapse", true);
		public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);
		public static readonly Texture2D SelectAll = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);
		public static readonly Texture2D CancelTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
	}
	public class ListFilterGroup : ListFilter
	{
		protected List<ListFilter> filters = new List<ListFilter>() { };
		protected bool any = true; // or all

		public override bool FilterApplies(Thing t) => 
			any ? filters.Any(f => f.AppliesTo(t)) : 
			filters.All(f => f.AppliesTo(t));

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref filters, "filters");
			Scribe_Values.Look(ref any, "any", true);
		}
		public override ListFilter Clone()
		{
			ListFilterGroup clone = (ListFilterGroup)base.Clone();
			clone.filters = filters.Select(f => f.Clone()).ToList();
			clone.any = any;
			return clone;
		}

		public override bool DrawOption(Rect rect)
		{
			bool beforeAny = any;
			WidgetRow row = new WidgetRow(rect.x, rect.y);
			row.Label("Include things that match");
			if (row.ButtonText(any ? "Any" : "All"))
				any = !any;
			row.Label("of these filters:");
			return any != beforeAny;
		}

		public override bool DrawMore(Listing_Standard outerListing)
		{
			//Get rect enough for each filter + Add row
			int neededRows = (1 + filters.Count);//No groups inside groups;
			Rect groupRect = outerListing.GetRect((Text.LineHeight + outerListing.verticalSpacing) * (1 + filters.Count));
			Rect nestedRect = groupRect.LeftPartPixels(Text.LineHeight);
			groupRect.xMin += Text.LineHeight + outerListing.verticalSpacing;
			outerListing.Gap();

			//Draw Indent
			Listing_Standard nestListing = new Listing_Standard();
			nestListing.Begin(nestedRect);
			foreach (var _ in Enumerable.Range(0, neededRows))
				nestListing.Label(":");// DrawTexture(TexButton.Reveal, Text.LineHeight, Text.LineHeight);
			nestListing.End();

			//Draw filters
			Listing_Standard groupListing = new Listing_Standard();
			groupListing.Begin(groupRect);
			bool changed = MainTabWindow_List.DoFilters(groupListing, filters);
			if (groupListing.ButtonImage(TexButton.Plus, Text.LineHeight, Text.LineHeight))
			{
				MainTabWindow_List.AddFilterFloat(filters, ListFilterMaker.Filter_Group);
			}
			groupListing.End();

			return changed;
		}
	}

	public class ListFilterInventory : ListFilterGroup
	{
		protected bool parent;//or child

		public override bool FilterApplies(Thing t)
		{
			if (parent)
			{
				IThingHolder parent = t.ParentHolder;
				while (parent.IsValidHolder())
				{
					if (parent is Thing parentThing && base.FilterApplies(parentThing))
						return true;
					parent = parent.ParentHolder;
				}
				return false;
			}
			else
			{
				return ContentsUtility.AllKnownThings(t as IThingHolder).Any(child => base.FilterApplies(child));
			}
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref parent, "parent", true);
		}
		public override ListFilter Clone()
		{
			ListFilterInventory clone = (ListFilterInventory)base.Clone();
			clone.parent = parent;
			return clone;
		}

		public override bool DrawOption(Rect rect)
		{
			bool changed = false;
			WidgetRow row = new WidgetRow(rect.x, rect.y);
			if (row.ButtonText(parent ? "The thing holding this" : "Anything this is holding"))
			{
				changed = true;
				parent = !parent;
			}
			row.Label("matches");
			if (row.ButtonText(any ? "Any" : "All"))
			{
				changed = true;
				any = !any;
			}
			row.Label("of:");
			return changed;
		}
	}


	public static class ListingEx
	{
		public static void DrawTexture(this Listing_Standard listing, Texture2D tex, float width, float height)
		{
			Rect texRect = listing.GetRect(height);
			texRect.width = width;
			GUI.DrawTexture(texRect, tex);
			listing.Gap(listing.verticalSpacing);
		}
	}
}
