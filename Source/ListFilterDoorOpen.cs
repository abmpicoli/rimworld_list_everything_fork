using Verse;
using RimWorld;


namespace List_Everything
{
  class ListFilterDoorOpen : ListFilterDropDown<DoorOpenFilter>
	{
		protected override bool FilterApplies(Thing thing)
		{
			Building_Door door = thing as Building_Door;
			if (door == null) return false;
			switch (sel)
			{
				case DoorOpenFilter.Open: return door.Open;
				case DoorOpenFilter.Close: return !door.Open;
				case DoorOpenFilter.HoldOpen: return door.HoldOpen;
				case DoorOpenFilter.BlockedOpenMomentary: return door.BlockedOpenMomentary;
			}
			return false;//???
		}
		public override string NameFor(DoorOpenFilter o)
		{
			switch (o)
			{
				case DoorOpenFilter.Open: return "TD.Opened".Translate();
				case DoorOpenFilter.Close: return "VentClosed".Translate();
				case DoorOpenFilter.HoldOpen: return "CommandToggleDoorHoldOpen".Translate().CapitalizeFirst();
				case DoorOpenFilter.BlockedOpenMomentary: return "TD.BlockedOpen".Translate();
			}
			return "???";
		}
	}
}
