using Verse;


namespace List_Everything
{
  class ListFilterDrawerType : ListFilterDropDown<DrawerType>
	{
		protected override bool FilterApplies(Thing thing) =>
			thing.def.drawerType == sel;
	}
}
