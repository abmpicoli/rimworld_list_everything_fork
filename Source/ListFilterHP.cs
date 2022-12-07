using Verse;
using UnityEngine;


namespace List_Everything
{
  class ListFilterHP : ListFilterWithOption<FloatRange>
	{
		public ListFilterHP() => sel = FloatRange.ZeroToOne;

		protected override bool FilterApplies(Thing thing)
		{
			float? pct = null;
			if (thing is Pawn pawn)
				pct = pawn.health.summaryHealth.SummaryHealthPercent;
			if (thing.def.useHitPoints)
				pct = (float)thing.HitPoints / thing.MaxHitPoints;
			return pct != null && sel.Includes(pct.Value);
		}

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
