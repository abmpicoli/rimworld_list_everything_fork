using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using System.IO;
using System.Collections;


namespace List_Everything
{
  public class ListFilterDef : ListFilterSelectableDef
	{
		public Type filterClass;

		public override IEnumerable<string> ConfigErrors()
		{
			if (filterClass == null)
				yield return "ListFilterDef needs filterClass set";
		}
	}

	public abstract partial class ListFilter : IExposable
	{
		public ListFilterDef def;

		public IFilterHolder parent;
		// parent is not set after ExposeData, that'll be done in Clone.
		// parent is only used in UI or actual processing so as is made clear below,
		// An ExpostData-loaded ListFilter needs to be cloned before actual use

		public FindDescription RootFindDesc => parent.RootFindDesc;


		protected int id; //For Widgets.draggingId purposes
		private static int nextID = 1;
		protected ListFilter() { id = nextID++; }


		private bool enabled = true; //simply turn off but keep in list
		public bool Enabled => enabled && DisableReason == null;

		private bool include = true; //or exclude


		// Okay, save/load. The basic gist here is:
		// ExposeData saves any filter fine.
		// ExposeData can load a filter for reference, but it's not yet usable.
		// After ExposeData loading, filters need to be cloned
		// After Cloning, they get DoResolveReference on a map
		// Then filters can actually be used.

		// Even if map is null and it's searching all maps,
		// Even if it's a def that could've been loaded already.
		// ResolveRef is when any named thing get resolved


		// Any overridden ExposeData+Clone should copy data but not process much.
		// If there's proessing to do, do it in ResolveReference. 
		// e.g. ListFilterWithOption sets refName in Clone,
		//  but sets the actual selection in ResolveReference

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref enabled, "enabled", true);
			Scribe_Values.Look(ref include, "include", true);
		}

		public virtual ListFilter Clone()
		{
			ListFilter clone = ListFilterMaker.MakeFilter(def);
			clone.enabled = enabled;
			clone.include = include;
			//clone.parent = newHolder; //No - MakeFilter just set it.
			return clone;
		}
		public virtual void DoResolveReference(Map map) { }


		public IEnumerable<Thing> Apply(IEnumerable<Thing> list)
		{
			return Enabled ? doApply(list) : list;
		}

		public virtual IEnumerable<Thing> doApply(IEnumerable<Thing> list)
		{
			return list.Where(t => AppliesTo(t));
		}

		//This can be problematic for minified things: We want the qualities of the inner things,
		// but position/status of outer thing. So it just checks both -- but then something like 'no stuff' always applies. Oh well
		public bool AppliesTo(Thing thing)
		{
			bool applies = FilterApplies(thing);
			if (!applies && thing.GetInnerThing() is Thing innerThing && innerThing != thing)
				applies = FilterApplies(innerThing);

			return applies == include;
		}

		protected abstract bool FilterApplies(Thing thing);




		private bool shouldFocus;
		public void Focus() => shouldFocus = true;
		protected virtual void DoFocus() { }


		// Seems to be GameFont.Small on load so we're good
		public static float? incExcWidth;
		public static float IncExcWidth =>
			incExcWidth.HasValue ? incExcWidth.Value :
			(incExcWidth = Mathf.Max(Text.CalcSize("TD.IncludeShort".Translate()).x, Text.CalcSize("TD.ExcludeShort".Translate()).x)).Value;

		public (bool, bool) Listing(Listing_StandardIndent listing, bool locked)
		{
			Rect rowRect = listing.GetRect(Text.LineHeight);
			WidgetRow row = new WidgetRow(rowRect.xMax, rowRect.y, UIDirection.LeftThenDown, rowRect.width);

			bool changed = false;
			bool delete = false;

			if (locked)
			{
				row.Label(include ? "TD.IncludeShort".Translate() : "TD.ExcludeShort".Translate(),
					IncExcWidth, "TD.IncludeOrExcludeThingsMatchingThisFilter".Translate());
				row.Gap(4);
			}
			else
			{
				//Clear button
				if (row.ButtonIcon(TexButton.CancelTex, "TD.DeleteThisFilter".Translate()))
				{
					delete = true;
					changed = true;
				}

				//Toggle button
				if (row.ButtonIcon(enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, "TD.EnableThisFilter".Translate()))
				{
					enabled = !enabled;
					changed = true;
				}

				//Include/Exclude
				if (row.ButtonText(include ? "TD.IncludeShort".Translate() : "TD.ExcludeShort".Translate(),
					"TD.IncludeOrExcludeThingsMatchingThisFilter".Translate(),
					fixedWidth: IncExcWidth))
				{
					include = !include;
					changed = true;
				}
			}


			//Draw option row
			rowRect.width -= (rowRect.xMax - row.FinalX);
			changed |= DrawMain(rowRect, locked);
			changed |= DrawUnder(listing, locked);
			if (shouldFocus)
			{
				DoFocus();
				shouldFocus = false;
			}
			if (DisableReason is string reason)
			{
				Widgets.DrawBoxSolid(rowRect, new Color(0.5f, 0, 0, 0.25f));

				TooltipHandler.TipRegion(rowRect, reason);
			}

			listing.Gap(listing.verticalSpacing);
			return (changed, delete);
		}


		public virtual bool DrawMain(Rect rect, bool locked)
		{
			Widgets.Label(rect, def.LabelCap);
			return false;
		}
		protected virtual bool DrawUnder(Listing_StandardIndent listing, bool locked) => false;

		public virtual bool ValidForAllMaps => true && !CurrentMapOnly;
		public virtual bool CurrentMapOnly => false;

		public virtual string DisableReason =>
			!ValidForAllMaps && RootFindDesc.allMaps
				? "TD.ThisFilterDoesntWorkWithAllMaps".Translate()
				: null;

		public static void DoFloatOptions(List<FloatMenuOption> options)
		{
			if (options.NullOrEmpty())
				Messages.Message("TD.ThereAreNoOptionsAvailablePerhapsYouShouldUncheckOnlyAvailableThings".Translate(), MessageTypeDefOf.RejectInput);
			else
				Find.WindowStack.Add(new FloatMenu(options));
		}

		public virtual bool Check(Predicate<ListFilter> check) => check(this);


		/**
		 * PostProcess allows changing the final list outcomes. This allows the use of "order by" clauses, "Limit X" clauses.
		 * PostProcess always happen AFTER the filters are applied.
		 */
		public IEnumerable<Thing> PostProcess(IEnumerable<Thing> allThings)
		{
			return allThings;
		}
	}

	class FloatMenuOptionAndRefresh : FloatMenuOption
	{
		ListFilter owner;
		public FloatMenuOptionAndRefresh(string label, Action action, ListFilter f) : base(label, action)
		{
			owner = f;
		}

		public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
		{
			bool result = base.DoGUI(rect, colonistOrdering, floatMenu);

			if (result)
				owner.RootFindDesc.RemakeList();

			return result;
		}
	}

	enum ForbiddenType { Forbidden, Allowed, Forbiddable }

	enum ListCategory
	{
		Person,
		Animal,
		Item,
		Building,
		Natural,
		Plant,
		Other
	}

	enum MineableType { Resource, Rock, All }

	enum BaseAreas { Home, BuildRoof, NoRoof, SnowClear };

	enum DoorOpenFilter { Open, Close, HoldOpen, BlockedOpenMomentary }
}
