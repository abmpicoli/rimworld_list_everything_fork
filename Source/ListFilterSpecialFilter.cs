using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterSpecialFilter : ListFilterDropDown<SpecialThingFilterDef>
	{
		public ListFilterSpecialFilter() => sel = SpecialThingFilterDefOf.AllowFresh;

		protected override bool FilterApplies(Thing thing) =>
			sel.Worker.Matches(thing);
	}
}
