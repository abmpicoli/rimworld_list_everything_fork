using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;


namespace List_Everything
{
  class ListFilterThingDef : ListFilterDropDown<ThingDef>
	{
		public IntRange stackRange;
		public ListFilterThingDef()
		{
			sel = ThingDefOf.WoodLog;
		}
		protected override void PostSelected()
		{
			stackRange.min = 1;
			stackRange.max = sel.stackLimit;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref stackRange, "stackRange");
		}
		public override ListFilter Clone()
		{
			ListFilterThingDef clone = (ListFilterThingDef)base.Clone();
			clone.stackRange = stackRange;
			return clone;
		}


		protected override bool FilterApplies(Thing thing) =>
			sel == thing.def &&
			(sel.stackLimit <= 1 || IntRangeUtils.Includes(stackRange,thing.stackCount));

		public override IEnumerable<ThingDef> Options() =>
			(ContentsUtility.OnlyAvailable ?
				ContentsUtility.AvailableInGame(t => t.def) :
				base.Options())
			.Where(def => FindDescription.ValidDef(def));

		public override bool Ordered => true;

		public override bool DrawCustom(Rect rect, WidgetRow row)
		{
			if (sel.stackLimit > 1)
			{
				IntRange newRange = stackRange;
				Widgets.IntRange(rect, id, ref newRange, 1, sel.stackLimit);
				if (newRange != stackRange)
				{
					stackRange = newRange;
					return true;
				}
			}
			return false;
		}
	}
}
