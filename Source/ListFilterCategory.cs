using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterCategory : ListFilterDropDown<ListCategory>
	{
		protected override bool FilterApplies(Thing thing)
		{
			switch (sel)
			{
				case ListCategory.Person: return thing is Pawn pawn && !pawn.NonHumanlikeOrWildMan();
				case ListCategory.Animal: return thing is Pawn animal && animal.NonHumanlikeOrWildMan();
				case ListCategory.Item: return thing.def.alwaysHaulable;
				case ListCategory.Building: return thing is Building building && building.def.filthLeaving != ThingDefOf.Filth_RubbleRock;
				case ListCategory.Natural: return thing is Building natural && natural.def.filthLeaving == ThingDefOf.Filth_RubbleRock;
				case ListCategory.Plant: return thing is Plant;
				case ListCategory.Other: return !(thing is Pawn) && !(thing is Building) && !(thing is Plant) && !thing.def.alwaysHaulable;
			}
			return false;
		}
	}
}
