using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace List_Everything
{
  class ListFilterClassType : ListFilterDropDown<Type>
	{
		public ListFilterClassType() => sel = typeof(Thing);

		protected override bool FilterApplies(Thing thing) =>
			sel.IsAssignableFrom(thing.GetType());

		public static List<Type> types = typeof(Thing).AllSubclassesNonAbstract().OrderBy(t => t.ToString()).ToList();
		public override IEnumerable<Type> Options() =>
			ContentsUtility.OnlyAvailable ?
				ContentsUtility.AvailableInGame(t => t.GetType()).OrderBy(NameFor).ToList() :
				types;
	}
}
