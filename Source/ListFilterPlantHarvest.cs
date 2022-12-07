using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterPlantHarvest : ListFilter
	{
		protected override bool FilterApplies(Thing thing) =>
			thing is Plant plant && plant.HarvestableNow;

	}
}
