using Verse;


namespace List_Everything
{
  class ListFilterOnScreen : ListFilter
	{
		protected override bool FilterApplies(Thing thing) =>
			thing.OccupiedRect().Overlaps(Find.CameraDriver.CurrentViewRect);

		public override bool CurrentMapOnly => true;
	}
}
