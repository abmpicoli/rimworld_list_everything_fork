using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterFreshness : ListFilterDropDown<RotStage>
	{
		protected override bool FilterApplies(Thing thing)
		{
			CompRottable rot = thing.TryGetComp<CompRottable>();
			return
				extraOption == 1 ? rot != null :
				extraOption == 2 ? GenTemperature.RotRateAtTemperature(thing.AmbientTemperature) is float r && r > 0 && r < 1 :
				extraOption == 3 ? GenTemperature.RotRateAtTemperature(thing.AmbientTemperature) <= 0 :
				rot?.Stage == sel;
				
		}

		public override string NameFor(RotStage o) => ("RotState" + o.ToString()).Translate();

		public override int ExtraOptionsCount => 3;
		public override string NameForExtra(int ex) =>
			ex == 1 ? "TD.Spoils".Translate() :
			ex == 2 ? "TD.Refrigerated".Translate() :
			"TD.Frozen".Translate();
	}
}
