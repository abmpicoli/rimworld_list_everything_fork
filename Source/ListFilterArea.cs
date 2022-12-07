using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterArea : ListFilterDropDown<Area>
	{
		public ListFilterArea()
		{
			extraOption = 1;
		}

		protected override Area ResolveReference(Map map) =>
			map.areaManager.GetLabeled(refName);

		public override bool ValidForAllMaps => extraOption > 0 || sel == null;

		protected override bool FilterApplies(Thing thing)
		{
			Map map = thing.MapHeld;
			IntVec3 pos = thing.PositionHeld;

			if (extraOption == 5)
				return pos.Roofed(map);

			if (extraOption == 0)
				return sel != null ? sel[pos] :
				map.areaManager.AllAreas.Any(a => a[pos]);

			switch ((BaseAreas)(extraOption - 1))
			{
				case BaseAreas.Home: return map.areaManager.Home[pos];
				case BaseAreas.BuildRoof: return map.areaManager.BuildRoof[pos];
				case BaseAreas.NoRoof: return map.areaManager.NoRoof[pos];
				case BaseAreas.SnowClear: return map.areaManager.SnowClear[pos];
			}
			return false;
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<Area> Options() => Find.CurrentMap.areaManager.AllAreas.Where(a => a is Area_Allowed);
		public override string NameFor(Area o) => o.Label;

		public override int ExtraOptionsCount => 5;
		public override string NameForExtra(int ex)
		{
			if (ex == 5) return "Roofed".Translate().CapitalizeFirst();
			switch ((BaseAreas)(ex - 1))
			{
				case BaseAreas.Home: return "Home".Translate();
				case BaseAreas.BuildRoof: return "BuildRoof".Translate().CapitalizeFirst();
				case BaseAreas.NoRoof: return "NoRoof".Translate().CapitalizeFirst();
				case BaseAreas.SnowClear: return "SnowClear".Translate().CapitalizeFirst();
			}
			return "???";
		}
	}
}
