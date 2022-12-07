using System.Collections.Generic;
using System.Linq;
using Verse;


namespace List_Everything
{
  class ListFilterModded : ListFilterDropDown<ModContentPack>
	{
		public ListFilterModded()
		{
			sel = LoadedModManager.RunningMods.First(mod => mod.IsCoreMod);
		}


		public override bool UsesRefName => true;
		protected override string MakeRefName() => sel.ToString();

		protected override ModContentPack ResolveReference(Map map) =>
			LoadedModManager.RunningMods.FirstOrDefault(mod => mod.PackageIdPlayerFacing == refName);


		protected override bool FilterApplies(Thing thing) =>
			sel == thing.ContentSource;

		public override IEnumerable<ModContentPack> Options() =>
			LoadedModManager.RunningMods.Where(mod => mod.AllDefs.Any(d => d is ThingDef));

		public override string NameFor(ModContentPack o) => o.Name;
	}
}
