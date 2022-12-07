using Verse;
using RimWorld;
using UnityEngine;


namespace List_Everything
{
  class ListFilterGrowth : ListFilterWithOption<FloatRange>
	{
		public ListFilterGrowth() => sel = FloatRange.ZeroToOne;

		protected override bool FilterApplies(Thing thing) =>
			thing is Plant p && sel.Includes(p.Growth);
		public override bool DrawMain(Rect rect, bool locked)
		{
			base.DrawMain(rect, locked);
			FloatRange newRange = sel;
			Widgets.FloatRange(rect.RightPart(0.5f), id, ref newRange, valueStyle: ToStringStyle.PercentZero);
			if (sel != newRange)
			{
				sel = newRange;
				return true;
			}
			return false;
		}
	}
}
