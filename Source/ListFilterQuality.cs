using Verse;
using RimWorld;
using UnityEngine;


namespace List_Everything
{
  class ListFilterQuality : ListFilterWithOption<QualityRange>
	{
		public ListFilterQuality() => sel = QualityRange.All;

		protected override bool FilterApplies(Thing thing) =>
			thing.TryGetQuality(out QualityCategory qc) &&
			sel.Includes(qc);

		public override bool DrawMain(Rect rect, bool locked)
		{
			base.DrawMain(rect, locked);
			QualityRange newRange = sel;
			Widgets.QualityRange(rect.RightPart(0.5f), id, ref newRange);
			if (sel != newRange)
			{
				sel = newRange;
				return true;
			}
			return false;
		}
	}
}
