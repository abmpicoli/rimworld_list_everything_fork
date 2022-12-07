using System.Collections.Generic;
using System.Linq;
using Verse;


namespace List_Everything
{
  public class ListFilterOrderByName : ListFilter
	{
		protected override bool FilterApplies(Thing thing)
		{
			return true;
		}
		public override IEnumerable<Thing> doApply(IEnumerable<Thing> list)
		{
			return list.OrderBy(thing => thing.def.label);
		}
	}
}
