using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace List_Everything
{
  public class ListFilterBySearchSpot : ListFilter
	{
		private Func<Thing, String> SortCriteria()
		{
			List<Building> allSearchSpots = new();
			foreach (Map map in Current.Game.Maps)
			{
				foreach (Building b in map.listerBuildings.allBuildingsColonist)
				{
					if (b.GetType() == typeof(DummySpot))
					{
						allSearchSpots.Add(b);
					}

				}
			}
			return (theThing) =>
			{
				double distance = 999999;
				foreach (Building b in allSearchSpots)
				{
					if (b.Map == theThing.Map)
					{
						distance = Math.Min(distance, b.Position.DistanceTo(theThing.Position));
					}
				}
				return (distance / 2).ToString("000000");
			};
		}

		public override IEnumerable<Thing> doApply(IEnumerable<Thing> list)
		{
			return list.OrderBy(SortCriteria());
		}

		protected override bool FilterApplies(Thing thing)
		{
			return true;
		}

	}
}
