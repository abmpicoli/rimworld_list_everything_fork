using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterFaction : ListFilterDropDown<FactionRelationKind>
	{
		public ListFilterFaction() => extraOption = 1;

		protected override bool FilterApplies(Thing thing) =>
			extraOption == 1 ? thing.Faction == Faction.OfPlayer :
			extraOption == 2 ? thing.Faction == Faction.OfMechanoids :
			extraOption == 3 ? thing.Faction == Faction.OfInsects :
			extraOption == 4 ? thing.Faction == null || thing.Faction.def.hidden :
			(thing.Faction is Faction fac && fac != Faction.OfPlayer && fac.PlayerRelationKind == sel);

		public override string NameFor(FactionRelationKind o) => o.GetLabel();

		public override int ExtraOptionsCount => 4;
		public override string NameForExtra(int ex) => // or FleshTypeDef but this works
			ex == 1 ? "TD.Player".Translate() :
			ex == 2 ? "TD.Mechanoid".Translate() :
			ex == 3 ? "TD.Insectoid".Translate() :
			"TD.NoFaction".Translate();
	}
}
