using System.Collections.Generic;
using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterItemCategory : ListFilterDropDown<ThingCategoryDef>
	{
		public ListFilterItemCategory() => sel = ThingCategoryDefOf.Root;

		protected override bool FilterApplies(Thing thing) =>
			thing.def.IsWithinCategory(sel);

		public override IEnumerable<ThingCategoryDef> Options() =>
			ContentsUtility.OnlyAvailable ?
				ContentsUtility.AvailableInGame(ThingCategoryDefsOfThing) :
				base.Options();

		public static IEnumerable<ThingCategoryDef> ThingCategoryDefsOfThing(Thing thing)
		{
			if (thing.def.thingCategories == null)
				yield break;
			foreach (var def in thing.def.thingCategories)
			{
				yield return def;
				foreach (var pDef in def.Parents)
					yield return pDef;
			}
		}
	}
}
