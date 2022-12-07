using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterDesignation : ListFilterDropDown<DesignationDef>
	{
		protected override bool FilterApplies(Thing thing) =>
			sel != null ?
			(sel.targetType == TargetType.Thing ? thing.MapHeld.designationManager.DesignationOn(thing, sel) != null :
			thing.MapHeld.designationManager.DesignationAt(thing.PositionHeld, sel) != null) :
			(thing.MapHeld.designationManager.DesignationOn(thing) != null ||
			thing.MapHeld.designationManager.AllDesignationsAt(thing.PositionHeld).Count() > 0);

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<DesignationDef> Options() =>
			ContentsUtility.OnlyAvailable ?
				Find.CurrentMap.designationManager.AllDesignations.Select(d => d.def).Distinct() :
				base.Options();

		public override bool Ordered => true;
		public override string NameFor(DesignationDef o) => o.defName; // no labels on Designation def
	}
}
