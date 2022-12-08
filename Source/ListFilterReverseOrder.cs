using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace List_Everything
{
  public class ListFilterReverseOrder: ListFilter
  {
		protected override bool FilterApplies(Thing thing)
		{
			return true;
		}
		public override IEnumerable<Thing> doApply(IEnumerable<Thing> list)
		{
			return list.Reverse();
		}

	}
}
