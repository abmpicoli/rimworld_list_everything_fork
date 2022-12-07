using Verse;
using RimWorld;
using UnityEngine;


namespace List_Everything
{
  class ListFilterTimeToRot : ListFilter
	{
		IntRange ticksRange = new IntRange(0, GenDate.TicksPerDay * 10);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksRange, "ticksRange");
		}
		public override ListFilter Clone()
		{
			ListFilterTimeToRot clone = (ListFilterTimeToRot)base.Clone();
			clone.ticksRange = ticksRange;
			return clone;
		}

		protected override bool FilterApplies(Thing thing) =>
			thing.TryGetComp<CompRottable>()?.TicksUntilRotAtCurrentTemp is int t && IntRangeUtils.Includes(ticksRange, t);

		public override bool DrawMain(Rect rect, bool locked)
		{
			base.DrawMain(rect, locked);

			IntRange newRange = ticksRange;
			Widgets.IntRange(rect.RightPart(0.5f), id, ref newRange, 0, GenDate.TicksPerDay * 20,
				$"{ticksRange.min * 1f / GenDate.TicksPerDay:0.0} - {ticksRange.max * 1f / GenDate.TicksPerDay:0.0}");
			if (newRange != ticksRange)
			{
				ticksRange = newRange;
				return true;
			}
			return false;
		}
	}
}
