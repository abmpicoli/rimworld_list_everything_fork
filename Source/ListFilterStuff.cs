using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterStuff : ListFilterDropDown<ThingDef>
	{
		protected override bool FilterApplies(Thing thing)
		{
			ThingDef stuff = thing is IConstructible c ? c.EntityToBuildStuff() : thing.Stuff;
			return
				extraOption == 1 ? !thing.def.MadeFromStuff :
				extraOption > 1 ? stuff?.stuffProps?.categories?.Contains(DefDatabase<StuffCategoryDef>.AllDefsListForReading[extraOption - 2]) ?? false :
				sel == null ? stuff != null :
				stuff == sel;
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<ThingDef> Options() =>
			ContentsUtility.OnlyAvailable
				? ContentsUtility.AvailableInGame(t => t.Stuff)
				: DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.IsStuff);

		public override int ExtraOptionsCount => DefDatabase<StuffCategoryDef>.DefCount + 1;
		public override string NameForExtra(int ex) =>
			ex == 1 ? "TD.NotMadeFromStuff".Translate() :
			DefDatabase<StuffCategoryDef>.AllDefsListForReading[ex - 2]?.LabelCap;
	}
}
