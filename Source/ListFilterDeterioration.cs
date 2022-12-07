using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterDeterioration : ListFilter
	{
		protected override bool FilterApplies(Thing thing) =>
			SteadyEnvironmentEffects.FinalDeteriorationRate(thing) >= 0.001f;
	}
}
