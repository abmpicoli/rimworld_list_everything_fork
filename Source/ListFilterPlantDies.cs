using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterPlantDies : ListFilter
	{
		protected override bool FilterApplies(Thing thing) =>
			thing is Plant plant && (plant.def.plant?.dieIfLeafless ?? false);
	}
}
