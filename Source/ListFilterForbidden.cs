using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterForbidden : ListFilterDropDown<ForbiddenType>
	{
		protected override bool FilterApplies(Thing thing)
		{
			bool forbiddable = thing.def.HasComp(typeof(CompForbiddable)) && thing.Spawned;
			if (!forbiddable) return false;
			bool forbidden = thing.IsForbidden(Faction.OfPlayer);
			switch (sel)
			{
				case ForbiddenType.Forbidden: return forbidden;
				case ForbiddenType.Allowed: return !forbidden;
			}
			return true;  //forbiddable
		}
	}
}
