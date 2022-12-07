using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterZone : ListFilterDropDown<Zone>
	{
		protected override Zone ResolveReference(Map map) =>
			map.zoneManager.AllZones.FirstOrDefault(z => z.label == refName);

		public override bool ValidForAllMaps => extraOption != 0 || sel == null;

		protected override bool FilterApplies(Thing thing)
		{
			IntVec3 pos = thing.PositionHeld;
			Zone zoneAtPos = thing.MapHeld.zoneManager.ZoneAt(pos);
			return
				extraOption == 1 ? zoneAtPos is Zone_Stockpile :
				extraOption == 2 ? zoneAtPos is Zone_Growing :
				sel != null ? zoneAtPos == sel :
				zoneAtPos != null;
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<Zone> Options() => Find.CurrentMap.zoneManager.AllZones;

		public override int ExtraOptionsCount => 2;
		public override string NameForExtra(int ex) => ex == 1 ? "TD.AnyStockpile".Translate() : "TD.AnyGrowingZone".Translate();
	}
}
