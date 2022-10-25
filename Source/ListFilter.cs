﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace List_Everything
{
	public class ListFilterDef : Def
	{
		public Type filterClass;
		public bool devOnly;

		public override IEnumerable<string> ConfigErrors()
		{
			if (GetType() != typeof(ListFilterListDef) && filterClass == typeof(ListFilterSelection))
				yield return "ListFilterSelection should be a ListFilterListDef";

			if (filterClass == null)
				yield return "ListFilterDef needs filterClass set";
		}
	}

	// There are too many filter subclasses to globally list them
	// So group them in lists (which is itself a subclass)
	// Then only the filters not nested under a list will be globally listed
	public class ListFilterListDef : ListFilterDef
	{
		private List<ListFilterDef> subFilters = null;
		public IEnumerable<ListFilterDef> SubFilters =>
			subFilters ?? Enumerable.Empty<ListFilterDef>();

		public override void PostLoad()
		{
			filterClass = typeof(ListFilterSelection);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (subFilters.NullOrEmpty())
				yield return "ListFilterListDef needs to set subFilters";
		}
	}

	[DefOf]
	[StaticConstructorOnStartup]
	public static class ListFilterMaker
	{
		public static ListFilterDef Filter_Name;
		public static ListFilterDef Filter_Group;

		public static ListFilter MakeFilter(ListFilterDef def, FindDescription owner)
		{
			ListFilter filter = (ListFilter)Activator.CreateInstance(def.filterClass);
			filter.def = def;
			filter.owner = owner;
			filter.PostMake();
			return filter;
		}
		public static ListFilter NameFilter(FindDescription owner) =>
			ListFilterMaker.MakeFilter(ListFilterMaker.Filter_Name, owner);


		// Filters that aren't grouped under a ListFilterListDef
		private static readonly List<ListFilterDef> rootFilters;

		static ListFilterMaker()
		{
			rootFilters = DefDatabase<ListFilterDef>.AllDefs.ToList();
			foreach (var listDef in DefDatabase<ListFilterListDef>.AllDefs)
				foreach (var subDef in listDef.SubFilters)	// ?? because game explodes on config error
					rootFilters.Remove(subDef);
		}

		public static IEnumerable<ListFilterDef> SelectableList =>
			rootFilters.Where(d => (Prefs.DevMode || !d.devOnly));
	}

	public abstract class ListFilter : IExposable
	{
		public int id; //For Widgets.draggingId purposes
		public static int nextID = 1;
		public ListFilterDef def;
		public FindDescription owner;

		protected ListFilter()  // Of course protected here doesn't make subclasses protected sooo ?
		{
			id = nextID++;
		}

		private bool enabled = true; //simply turn off but keep in list
		public bool Enabled
		{
			get => enabled && DisableReason == null;
		}
		public bool include = true; //or exclude

		// Top level, as in, it's a root filter, it's not nested under another filter
		public bool topLevel = true;
		public bool delete;

		public IEnumerable<Thing> Apply(IEnumerable<Thing> list)
		{
			return Enabled ? list.Where(t => AppliesTo(t)) : list;
		}

		//This can be problematic for minified things: We want the qualities of the inner things,
		// but position/status of outer thing. So it just checks both -- but then something like 'no stuff' always applies. Oh well
		public bool AppliesTo(Thing thing)
		{
			bool applies = FilterApplies(thing);
			if (!applies && thing.GetInnerThing() is Thing innerThing && innerThing != thing)
				applies = FilterApplies(innerThing);

			return applies == include;
		}

		public abstract bool FilterApplies(Thing thing);


		private bool shouldFocus;
		public void Focus() => shouldFocus = true;
		protected virtual void DoFocus() { }

		public bool Listing(Listing_StandardIndent listing)
		{
			Rect rowRect = listing.GetRect(Text.LineHeight);
			WidgetRow row = new WidgetRow(rowRect.xMax, rowRect.y, UIDirection.LeftThenDown, rowRect.width);

			bool changed = false;

			if (owner.locked)
			{
				row.Label(include ? "TD.IncludeShort".Translate() : "TD.ExcludeShort".Translate());
			}
			else
			{
				//Clear button
				if (row.ButtonIcon(TexButton.CancelTex, "TD.DeleteThisFilter".Translate()))
				{
					delete = true;
					changed = true;
				}

				//Toggle button
				if (row.ButtonIcon(enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, "TD.EnableThisFilter".Translate()))
				{
					enabled = !enabled;
					changed = true;
				}

				//Include/Exclude
				if (row.ButtonText(include ? "TD.IncludeShort".Translate() : "TD.ExcludeShort".Translate(), "TD.IncludeOrExcludeThingsMatchingThisFilter".Translate()))
				{
					include = !include;
					changed = true;
				}
			}


			//Draw option row
			rowRect.width -= (rowRect.xMax - row.FinalX);
			changed |= DrawOption(rowRect);
			changed |= DrawMore(listing);
			if (shouldFocus)
			{
				DoFocus();
				shouldFocus = false;
			}
			if (DisableReason is string reason)
			{
				Widgets.DrawBoxSolid(rowRect, new Color(0.5f, 0, 0, 0.25f));

				TooltipHandler.TipRegion(rowRect, reason);
			}

			listing.Gap(listing.verticalSpacing);
			return changed;
		}


		public virtual bool DrawOption(Rect rect)
		{
			if (topLevel) Widgets.Label(rect, def.LabelCap);
			return false;
		}
		public virtual bool DrawMore(Listing_StandardIndent listing) => false;

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref enabled, "enabled", true);
			Scribe_Values.Look(ref include, "include", true);
			Scribe_Values.Look(ref topLevel, "topLevel", true);
		}

		//Clone, and resolve references if map specified
		public virtual ListFilter Clone(Map map, FindDescription newOwner) =>
			BaseClone(map, newOwner);

		protected ListFilter BaseClone(Map map, FindDescription newOwner)
		{
			ListFilter clone = ListFilterMaker.MakeFilter(def, newOwner);
			clone.enabled = enabled;
			clone.include = include;
			clone.topLevel = topLevel;
			//clone.owner = newOwner; //No - MakeFilter sets it.
			return clone;
		}

		// PostMake called after the subclass constructor if you need the Def.
		public virtual void PostMake() { }

		public virtual bool ValidForAllMaps => true;

		public virtual string DisableReason =>
			!ValidForAllMaps && owner.allMaps
				? "TD.ThisFilterDoesntWorkWithAllMaps".Translate()
				: null;
	}

	class ListFilterName : ListFilterWithOption<string>
	{
		public ListFilterName() => sel = "";

		public override bool FilterApplies(Thing thing) =>
			//thing.Label.Contains(sel, CaseInsensitiveComparer.DefaultInvariant);	//Contains doesn't accept comparer with strings. okay.
			sel == "" || thing.Label.IndexOf(sel, StringComparison.OrdinalIgnoreCase) >= 0;

		public override bool DrawOption(Rect rect)
		{
			if (GUI.GetNameOfFocusedControl() == $"LIST_FILTER_NAME_INPUT{id}" &&
				Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
			{
				GUI.FocusControl("");
				Event.current.Use();
			}

			GUI.SetNextControlName($"LIST_FILTER_NAME_INPUT{id}");
			string newStr = Widgets.TextField(rect.LeftPart(0.9f), sel);
			if (newStr != sel)
			{
				sel = newStr;
				return true;
			}
			if (Widgets.ButtonImage(rect.RightPartPixels(rect.height), TexUI.RotLeftTex))
			{
				GUI.FocusControl("");
				sel = "";
				return true;
			}
			return false;
		}

		protected override void DoFocus()
		{
			GUI.FocusControl($"LIST_FILTER_NAME_INPUT{id}");
		}
	}

	enum ForbiddenType{ Forbidden, Allowed, Forbiddable}
	class ListFilterForbidden : ListFilterDropDown<ForbiddenType>
	{
		public override bool FilterApplies(Thing thing)
		{
			bool forbiddable = thing.def.HasComp(typeof(CompForbiddable)) && thing.Spawned;
			if (!forbiddable) return false;
			bool forbidden = thing.IsForbidden(Faction.OfPlayer);
			switch (sel)
			{
				case ForbiddenType.Forbidden: return forbidden;
				case ForbiddenType.Allowed: return !forbidden;
			}
			return true;  //forbiddable
		}
	}

	//automated ExposeData + Clone 
	public abstract class ListFilterWithOption<T> : ListFilter
	{
		protected T sel;//selection

		//References must be saved by name in case of T: ILoadReferenceable, since they not game specific
		//(probably could be in ListFilter)
		//ExposeData saves refName instead of sel
		//Only saved lists get ExposeData called, so only saved lists have refName set
		//(The in-game list will NOT have this set; The saved lists will have this set)
		//Saving the list will generate refName from the current filter on Clone(null) via MakeRefName()
		//Loading will use refName from the saved list to resolve references in Clone(map) via ResolveReference()
		//Cloning between two reference types makes the ref from current map and resolves on the new map
		string refName;

		public override void ExposeData()
		{
			base.ExposeData();

			//Oh Jesus T can be anything but Scribe doesn't like that much flexibility so here we are:
			if (typeof(Def).IsAssignableFrom(typeof(T)))
			{
				//From Scribe_Collections:
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					Def temp = sel as Def;
					Scribe_Defs.Look(ref temp, "sel");
				}
				else if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					//Scribe_Defs.Look doesn't work since it needs the subtype of "Def" and T isn't boxed to be a Def so DefFromNodeUnsafe instead
					sel = ScribeExtractor.DefFromNodeUnsafe<T>(Scribe.loader.curXmlParent["sel"]);
				}
			}
			else if (typeof(ILoadReferenceable).IsAssignableFrom(typeof(T)))
			{
				//Of course between games you can't get references so just save by name should be good enough.
				//objects saved here need to be copies made with Clone(null)
				Scribe_Values.Look(ref refName, "refName"); //And Clone will handle references
			}
			else if (typeof(IExposable).IsAssignableFrom(typeof(T)))
			{
				Scribe_Deep.Look(ref sel, "sel");
			}
			else
				Scribe_Values.Look(ref sel, "sel");
		}
		public override ListFilter Clone(Map map, FindDescription newOwner)
		{
			ListFilterWithOption<T> clone = (ListFilterWithOption<T>)base.Clone(map, newOwner);

			if (typeof(ILoadReferenceable).IsAssignableFrom(typeof(T)))
			{
				if (map == null)//SAVING: I don't have refName, but I make it and tell saved clone
				{
					clone.refName = sel == null ? "null" : MakeRefName();
				}
				else //LOADING: use my refName to resolve loaded clone's reference
				{
					if (refName == "null")
					{
						clone.sel = default(T);
					}
					{
						if (refName == null)
							clone.ResolveReference(MakeRefName(), map);//Cloning from ref to ref
						else
							clone.ResolveReference(refName, map);

						if (clone.sel == null)
							Messages.Message("TD.TriedToLoad0FilterNamed1ButTheCurrentMapDoesntHaveAnyByThatName".Translate(def.LabelCap, refName), MessageTypeDefOf.RejectInput);
					}
				}
			}
			else
				clone.sel = sel;

			return clone;
		}
		public virtual string MakeRefName() => sel.ToString();
		public virtual void ResolveReference(string refName, Map map) => throw new NotImplementedException();
	}

	abstract class ListFilterDropDown<T> : ListFilterWithOption<T>
	{
		public int extraOption; //0 meaning use T, 1+ defined in subclass
		public string selectionError; // Probably set on load when selection is invalid (missing mod?)
		public override string DisableReason => base.DisableReason ?? selectionError;

		// A subclass with extra fields needs to override ExposeData and Clone to copy them, like extraOption:
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref extraOption, "ex");
		}
		public override ListFilter Clone(Map map, FindDescription newOwner)
		{
			ListFilterDropDown<T> clone =
				extraOption == 0 ?
				(ListFilterDropDown<T>)base.Clone(map, newOwner) ://ListFilterwithOption handles sel, and refName for sel if needed
				(ListFilterDropDown<T>)BaseClone(map, newOwner);  //This is not needed with extraOption, so bypass ListFilterWithOption<T> to ListFilter
			clone.extraOption = extraOption;
			clone.selectionError = selectionError;
			return clone;
		}
		protected virtual void PostChosen()
		{
			// A subclass with fields whose validity depends on the selection should override this
			// Most common usage is to set a default value that is valid for the selection
			// e.g. the skill filter has a range 0-20, but that's valid for all skills, so no need to reset here
			// e.g. the hediff filter has a range too, but that depends on the selected hediff, so the selected range needs to be set here

			// extraOption is set = 0 before PostChosen() is called, so subclasses need not base.PostChosen() just for that.
		}

		private string GetLabel()
		{
			if (selectionError != null)
				return selectionError;

			if (extraOption > 0)
				return NameForExtra(extraOption);

			if (sel != null)
				return NameFor(sel);

			return NullOption() ?? "??Null selection??";
		}

		// This method works double duty:
		// Both telling if Sel can be set to null, and the string to show for null selection
		public virtual string NullOption() => null;

		public virtual IEnumerable<T> Options()
		{
			if (typeof(T).IsEnum)
				return Enum.GetValues(typeof(T)).OfType<T>();
			if (typeof(Def).IsAssignableFrom(typeof(T)))
				return GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(T)).Cast<T>();
			throw new NotImplementedException();
		}

		public virtual bool Ordered => false;
		public virtual string NameFor(T o) => o is Def def ? def.LabelCap.Resolve() : typeof(T).IsEnum ? o.TranslateEnum() : o.ToString();
		public override string MakeRefName() => NameFor(sel); //refname should not apply for defs or enums so this'll be ^^ o.ToString()

		public virtual int ExtraOptionsCount => 0;
		private IEnumerable<int> ExtraOptions() => Enumerable.Range(1, ExtraOptionsCount);
		public virtual string NameForExtra(int ex) => throw new NotImplementedException();

		private void ChooseSelected(T o)
		{
			sel = o;
			extraOption = 0;
			selectionError = null;//Right?
			PostChosen(); //If null is valid, subclass should know to handle it in PostChosen, and wherever Sel is
		}

		private void ChooseExtra(int ex)
		{
			sel = default;
			extraOption = ex;
			selectionError = null;//Right?
		}

		public override bool DrawOption(Rect rect)
		{
			bool changeSelection = false;
			bool changed = false;
			if (HasSpecial)
			{
				// No label, selected option button on left, special on right
				WidgetRow row = new WidgetRow(rect.x, rect.y);
				changeSelection = row.ButtonText(GetLabel());

				rect.xMin = row.FinalX;
				changed = DrawSpecial(rect, row);
			}
			else
			{
				//Just the label on left, and selected option button on right
				base.DrawOption(rect);
				changeSelection = Widgets.ButtonText(rect.RightPart(0.4f), GetLabel());
			}
			if (changeSelection)
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();

				if (NullOption() is string nullOption)
					options.Add(new FloatMenuOption(nullOption, () => ChooseSelected(default(T))));

				foreach (T o in Ordered ? Options().OrderBy(o => NameFor(o)) : Options())
					options.Add(new FloatMenuOption(NameFor(o), () => ChooseSelected(o)));

				foreach (int ex in ExtraOptions())
					options.Add(new FloatMenuOption(NameForExtra(ex), () => ChooseExtra(ex)));

				MainTabWindow_List.DoFloatMenu(options);

				changed = true;
			}
			return changed;
		}

		// Subclass can override DrawSpecial to draw anything custom
		// (otherwise it's just label and option selection button)
		// Use either rect or WidgetRow in the implementation
		public virtual bool DrawSpecial(Rect rect, WidgetRow row) => throw new NotImplementedException();

		// Auto detection of subclasses that use it:
		private static readonly HashSet<Type> specialDrawers = null;
		private bool HasSpecial => specialDrawers?.Contains(GetType()) ?? false;
		static ListFilterDropDown()//<T>	//Remember there's a specialDrawers for each <T> but functionally that doesn't change anything
		{
			foreach (Type subclass in typeof(ListFilterDropDown<T>).AllSubclassesNonAbstract())
			{
				if (subclass.GetMethod(nameof(DrawSpecial)).DeclaringType == subclass)
				{
					if(specialDrawers == null)
						specialDrawers = new HashSet<Type>();

					specialDrawers.Add(subclass);
				}
			}
		}
	}

	class ListFilterDesignation : ListFilterDropDown<DesignationDef>
	{
		public override bool FilterApplies(Thing thing) =>
			sel != null ?
			(sel.targetType == TargetType.Thing ? thing.MapHeld.designationManager.DesignationOn(thing, sel) != null :
			thing.MapHeld.designationManager.DesignationAt(thing.PositionHeld, sel) != null) :
			(thing.MapHeld.designationManager.DesignationOn(thing) != null ||
			thing.MapHeld.designationManager.AllDesignationsAt(thing.PositionHeld).Count() > 0);

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<DesignationDef> Options() =>
			ContentsUtility.onlyAvailable ?
				Find.CurrentMap.designationManager.AllDesignations.Select(d => d.def).Distinct() :
				base.Options();

		public override bool Ordered => true;
		public override string NameFor(DesignationDef o) => o.defName; // no labels on Designation def
	}

	class ListFilterFreshness : ListFilterDropDown<RotStage>
	{
		public override bool FilterApplies(Thing thing)
		{
			CompRottable rot = thing.TryGetComp<CompRottable>();
			return 
				extraOption == 1 ? rot != null : 
				extraOption == 2 ? GenTemperature.RotRateAtTemperature(thing.AmbientTemperature) is float r && r>0 && r<1 : 
				extraOption == 3 ? GenTemperature.RotRateAtTemperature(thing.AmbientTemperature) <= 0 : 
				rot?.Stage == sel;
		}

		public override string NameFor(RotStage o) => ("RotState"+o.ToString()).Translate();

		public override int ExtraOptionsCount => 3;
		public override string NameForExtra(int ex) =>
			ex == 1 ? "TD.Spoils".Translate() :
			ex == 2 ? "TD.Refrigerated".Translate() : 
			"TD.Frozen".Translate();
	}

	class ListFilterGrowth : ListFilterWithOption<FloatRange>
	{
		public ListFilterGrowth() => sel = FloatRange.ZeroToOne;

		public override bool FilterApplies(Thing thing) =>
			thing is Plant p && sel.Includes(p.Growth);
		public override bool DrawOption(Rect rect)
		{
			base.DrawOption(rect);
			FloatRange newRange = sel;
			Widgets.FloatRange(rect.RightPart(0.5f), id, ref newRange, valueStyle: ToStringStyle.PercentZero);
			if (sel != newRange)
			{
				sel = newRange;
				return true;
			}
			return false;
		}
	}

	class ListFilterPlantHarvest : ListFilter
	{
		public override bool FilterApplies(Thing thing) =>
			thing is Plant plant && plant.HarvestableNow;
	}

	class ListFilterPlantDies : ListFilter
	{
		public override bool FilterApplies(Thing thing) =>
			thing is Plant plant && (plant.def.plant?.dieIfLeafless ?? false);
	}

	class ListFilterClassType : ListFilterDropDown<Type>
	{
		public ListFilterClassType() => sel = typeof(Thing);

		public override bool FilterApplies(Thing thing) =>
			sel.IsAssignableFrom(thing.GetType());

		public static List<Type> types = typeof(Thing).AllSubclassesNonAbstract().OrderBy(t=>t.ToString()).ToList();
		public override IEnumerable<Type> Options() =>
			ContentsUtility.onlyAvailable ?
				ContentsUtility.AvailableInGame(t => t.GetType()).OrderBy(NameFor).ToList() : 
				types;
	}

	class ListFilterFaction : ListFilterDropDown<FactionRelationKind>
	{
		public ListFilterFaction() => extraOption = 1;

		public override bool FilterApplies(Thing thing) =>
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
	
	class ListFilterItemCategory : ListFilterDropDown<ThingCategoryDef>
	{
		public ListFilterItemCategory() => sel = ThingCategoryDefOf.Root;

		public override bool FilterApplies(Thing thing) =>
			thing.def.IsWithinCategory(sel);

		public override IEnumerable<ThingCategoryDef> Options() =>
			ContentsUtility.onlyAvailable ?
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

	class ListFilterSpecialFilter : ListFilterDropDown<SpecialThingFilterDef>
	{
		public ListFilterSpecialFilter() => sel = SpecialThingFilterDefOf.AllowFresh;

		public override bool FilterApplies(Thing thing) =>
			sel.Worker.Matches(thing);
	}

	enum ListCategory
	{
		Person,
		Animal,
		Item,
		Building,
		Natural,
		Plant,
		Other
	}
	class ListFilterCategory : ListFilterDropDown<ListCategory>
	{
		public override bool FilterApplies(Thing thing)
		{
			switch(sel)
			{
				case ListCategory.Person: return thing is Pawn pawn && !pawn.NonHumanlikeOrWildMan();
				case ListCategory.Animal: return thing is Pawn animal && animal.NonHumanlikeOrWildMan();
				case ListCategory.Item: return thing.def.alwaysHaulable;
				case ListCategory.Building: return thing is Building building && building.def.filthLeaving != ThingDefOf.Filth_RubbleRock;
				case ListCategory.Natural: return thing is Building natural && natural.def.filthLeaving == ThingDefOf.Filth_RubbleRock;
				case ListCategory.Plant: return thing is Plant;
				case ListCategory.Other: return !(thing is Pawn) && !(thing is Building) && !(thing is Plant) && !thing.def.alwaysHaulable;
			}
			return false;
		}
	}

	enum MineableType { Resource, Rock, All }
	class ListFilterMineable : ListFilterDropDown<MineableType>
	{
		public override bool FilterApplies(Thing thing)
		{
			switch (sel)
			{
				case MineableType.Resource: return thing.def.building?.isResourceRock ?? false;
				case MineableType.Rock: return (thing.def.building?.isNaturalRock ?? false) && (!thing.def.building?.isResourceRock ?? true);
				case MineableType.All: return thing.def.mineable;
			}
			return false;
		}
	}

	class ListFilterHP : ListFilterWithOption<FloatRange>
	{
		public ListFilterHP() => sel = FloatRange.ZeroToOne;

		public override bool FilterApplies(Thing thing)
		{
			float? pct = null;
			if (thing is Pawn pawn)
				pct = pawn.health.summaryHealth.SummaryHealthPercent;
			if (thing.def.useHitPoints)
				pct = (float)thing.HitPoints / thing.MaxHitPoints;
			return pct != null && sel.Includes(pct.Value);
		}

		public override bool DrawOption(Rect rect)
		{
			base.DrawOption(rect);
			FloatRange newRange = sel;
			Widgets.FloatRange(rect.RightPart(0.5f), id, ref newRange, valueStyle: ToStringStyle.PercentZero);
			if (sel != newRange)
			{
				sel = newRange;
				return true;
			}
			return false;
		}
	}

	class ListFilterQuality : ListFilterWithOption<QualityRange>
	{
		public ListFilterQuality() => sel = QualityRange.All;

		public override bool FilterApplies(Thing thing) =>
			thing.TryGetQuality(out QualityCategory qc) &&
			sel.Includes(qc);

		public override bool DrawOption(Rect rect)
		{
			base.DrawOption(rect);
			QualityRange newRange = sel;
			Widgets.QualityRange(rect.RightPart(0.5f), id, ref newRange);
			if (sel != newRange)
			{
				sel = newRange;
				return true;
			}
			return false;
		}
	}

	class ListFilterStuff : ListFilterDropDown<ThingDef>
	{
		public override bool FilterApplies(Thing thing)
		{
			ThingDef stuff = thing is IConstructible c ? c.EntityToBuildStuff() : thing.Stuff;
			return 
				extraOption == 1 ? !thing.def.MadeFromStuff :
				extraOption > 1 ?	stuff?.stuffProps?.categories?.Contains(DefDatabase<StuffCategoryDef>.AllDefsListForReading[extraOption - 2]) ?? false :
				sel == null ? stuff != null :
				stuff == sel;
		}
		
		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<ThingDef> Options() => 
			ContentsUtility.onlyAvailable
				? ContentsUtility.AvailableInGame(t => t.Stuff)
				: DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.IsStuff);
		
		public override int ExtraOptionsCount => DefDatabase<StuffCategoryDef>.DefCount + 1;
		public override string NameForExtra(int ex) =>
			ex == 1 ? "TD.NotMadeFromStuff".Translate() : 
			DefDatabase<StuffCategoryDef>.AllDefsListForReading[ex-2]?.LabelCap;
	}

	class ListFilterDrawerType : ListFilterDropDown<DrawerType>
	{
		public override bool FilterApplies(Thing thing) =>
			thing.def.drawerType == sel;
	}

	class ListFilterMissingBodyPart : ListFilterDropDown<BodyPartDef>
	{
		public override bool FilterApplies(Thing thing)
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
			ContentsUtility.onlyAvailable
				? ContentsUtility.AvailableInGame(
					t => (t as Pawn)?.health.hediffSet.GetMissingPartsCommonAncestors().Select(h => h.Part.def) ?? Enumerable.Empty<BodyPartDef>())
				: base.Options();

		public override int ExtraOptionsCount => 1;
		public override string NameForExtra(int ex) => "None".Translate();
	}


	enum BaseAreas { Home, BuildRoof, NoRoof, SnowClear };
	class ListFilterArea : ListFilterDropDown<Area>
	{
		public ListFilterArea()
		{
			extraOption = 1;
		}

		public override void ResolveReference(string refName, Map map) =>
			sel = map.areaManager.GetLabeled(refName);

		public override bool ValidForAllMaps => extraOption > 0 || sel == null;

		public override bool FilterApplies(Thing thing)
		{
			Map map = thing.MapHeld;
			IntVec3 pos = thing.PositionHeld;

			if (extraOption == 5)
				return pos.Roofed(map);

			if(extraOption == 0)
				return sel != null ? sel[pos] :
				map.areaManager.AllAreas.Any(a => a[pos]);

			switch((BaseAreas)(extraOption - 1))
			{
				case BaseAreas.Home:			return map.areaManager.Home[pos];
				case BaseAreas.BuildRoof: return map.areaManager.BuildRoof[pos];
				case BaseAreas.NoRoof:		return map.areaManager.NoRoof[pos];
				case BaseAreas.SnowClear: return map.areaManager.SnowClear[pos];
			}
			return false;
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<Area> Options() => Find.CurrentMap.areaManager.AllAreas.Where(a => a is Area_Allowed);
		public override string NameFor(Area o) => o.Label;

		public override int ExtraOptionsCount => 5;
		public override string NameForExtra(int ex)
		{
			if (ex == 5) return "Roofed".Translate().CapitalizeFirst();
			switch((BaseAreas)(ex - 1))
			{
				case BaseAreas.Home: return "Home".Translate();
				case BaseAreas.BuildRoof: return "BuildRoof".Translate().CapitalizeFirst();
				case BaseAreas.NoRoof: return "NoRoof".Translate().CapitalizeFirst();
				case BaseAreas.SnowClear: return "SnowClear".Translate().CapitalizeFirst();
			}
			return "???";
		}
	}

	class ListFilterZone : ListFilterDropDown<Zone>
	{
		public override void ResolveReference(string refName, Map map) =>
			sel = map.zoneManager.AllZones.FirstOrDefault(z => z.label == refName);

		public override bool ValidForAllMaps => extraOption != 0 || sel == null;

		public override bool FilterApplies(Thing thing)
		{
			IntVec3 pos = thing.PositionHeld;
			Zone zoneAtPos = thing.MapHeld.zoneManager.ZoneAt(pos);
			return 
				extraOption == 1 ? zoneAtPos is Zone_Stockpile :
				extraOption == 2 ? zoneAtPos is Zone_Growing :
				sel != null ? zoneAtPos == sel :
				zoneAtPos != null;
		}

		public override string NullOption() => "TD.AnyOption".Translate();
		public override IEnumerable<Zone> Options() => Find.CurrentMap.zoneManager.AllZones;

		public override int ExtraOptionsCount => 2;
		public override string NameForExtra(int ex) => ex == 1 ? "TD.AnyStockpile".Translate() : "TD.AnyGrowingZone".Translate();
	}

	class ListFilterDeterioration : ListFilter
	{
		public override bool FilterApplies(Thing thing) =>
			SteadyEnvironmentEffects.FinalDeteriorationRate(thing) >= 0.001f;
	}

	enum DoorOpenFilter { Open, Close, HoldOpen, BlockedOpenMomentary }
	class ListFilterDoorOpen : ListFilterDropDown<DoorOpenFilter>
	{
		public override bool FilterApplies(Thing thing)
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

	class ListFilterThingDef : ListFilterDropDown<ThingDef>
	{
		public ListFilterThingDef()
		{
			sel = ThingDefOf.WoodLog;
		}

		public override bool FilterApplies(Thing thing) =>
			sel == thing.def;

		public override IEnumerable<ThingDef> Options() =>
			(ContentsUtility.onlyAvailable ?
				ContentsUtility.AvailableInGame(t => t.def) :
				base.Options())
			.Where(def => FindDescription.ValidDef(def));

		public override bool Ordered => true;
	}

	class ListFilterModded : ListFilterDropDown<ModContentPack>
	{
		private string packageId;	//kept in case loaded from a missing mod, so it can be saved back out.

		public ListFilterModded()
		{
			sel = LoadedModManager.RunningMods.First(mod => mod.IsCoreMod);
		}

		public override void ExposeData()
		{
			base.ExposeData();

			if (sel == null)
			{
				// If loading, save the requested string directly into packageId
				// If saving and it was null, it must be loaded missing mod. packageId was saved so save that string directly.
				Scribe_Values.Look(ref packageId, "sel");

				// Also if loading, show error state
				if (Scribe.mode == LoadSaveMode.LoadingVars)
					selectionError = $"Missing: {packageId}?";
			}

		}
		public override ListFilter Clone(Map map, FindDescription newOwner)
		{
			ListFilterModded clone = (ListFilterModded)base.Clone(map, newOwner);
			clone.packageId = packageId;
			return clone;
		}

		public override bool FilterApplies(Thing thing) =>
			sel == thing.ContentSource;

		public override IEnumerable<ModContentPack> Options() =>
			LoadedModManager.RunningMods.Where(mod => mod.AllDefs.Any(d => d is ThingDef));

		public override string NameFor(ModContentPack o) => o.Name;


		static ListFilterModded()
		{
			ParseHelper.Parsers<ModContentPack>.Register(ParseModContentPack);
		}

		public static ModContentPack ParseModContentPack(string packageId) =>
			LoadedModManager.RunningMods.FirstOrDefault(mod => mod.PackageIdPlayerFacing == packageId);
	}

}
