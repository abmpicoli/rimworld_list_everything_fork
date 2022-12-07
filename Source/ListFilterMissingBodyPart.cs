using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterMissingBodyPart : ListFilterDropDown<BodyPartDef>
	{
		protected override bool FilterApplies(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn == null) return false;

			return
				extraOption == 1 ? pawn.health.hediffSet.GetMissingPartsCommonAncestors().NullOrEmpty() :
				sel == null ? !pawn.health.hediffSet.GetMissingPartsCommonAncestors().NullOrEmpty() :
				pawn.RaceProps.body.GetPartsWithDef(sel).Any(r => pawn.health.hediffSet.PartIsMissing(r));
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<BodyPartDef> Options() =>
			ContentsUtility.OnlyAvailable
				? ContentsUtility.AvailableInGame(
					t => (t as Pawn)?.health.hediffSet.GetMissingPartsCommonAncestors().Select(h => h.Part.def) ?? Enumerable.Empty<BodyPartDef>())
				: base.Options();

		public override int ExtraOptionsCount => 1;
		public override string NameForExtra(int ex) => "None".Translate();
	}
}
