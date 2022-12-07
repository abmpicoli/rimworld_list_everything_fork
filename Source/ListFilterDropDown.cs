using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;


namespace List_Everything
{
  abstract class ListFilterDropDown<T> : ListFilterWithOption<T>
	{
		private string GetLabel()
		{
			if (selectionError != null)
				return refName;

			if (extraOption > 0)
				return NameForExtra(extraOption);

			if (sel != null)
				return NameFor(sel);

			return NullOption() ?? "??Null selection??";
		}

		public virtual IEnumerable<T> Options()
		{
			if (IsEnum)
				return Enum.GetValues(typeof(T)).OfType<T>();
			if (IsDef)
				return GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(T)).Cast<T>();
			throw new NotImplementedException();
		}

		public virtual bool Ordered => false;
		public virtual string NameFor(T o) => o is Def def ? def.LabelCap.RawText : typeof(T).IsEnum ? o.TranslateEnum() : o.ToString();
		protected override string MakeRefName()
		{
			if (sel is Def def)
				return def.defName;

			// Many subclasses will just use NameFor, so do it here.
			return sel != null ? NameFor(sel) : base.MakeRefName();
		}

		public virtual int ExtraOptionsCount => 0;
		private IEnumerable<int> ExtraOptions() => Enumerable.Range(1, ExtraOptionsCount);
		public virtual string NameForExtra(int ex) => throw new NotImplementedException();

		public override bool DrawMain(Rect rect, bool locked)
		{
			bool changeSelection = false;
			bool changed = false;
			if (HasSpecial)
			{
				// Label, Selection option button on left, special on the remaining rect
				WidgetRow row = new WidgetRow(rect.x, rect.y);
				row.Label(def.LabelCap);
				changeSelection = row.ButtonText(GetLabel());

				rect.xMin = row.FinalX;
				changed = DrawCustom(rect, row);
			}
			else
			{
				//Just the label on left, and selected option button on right
				base.DrawMain(rect, locked);
				changeSelection = Widgets.ButtonText(rect.RightPart(0.4f), GetLabel());
			}
			if (changeSelection)
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();

				if (NullOption() is string nullOption)
					options.Add(new FloatMenuOptionAndRefresh(nullOption, () => sel = default, this)); //can't null because T isn't bound as reftype

				foreach (T o in Ordered ? Options().OrderBy(o => NameFor(o)) : Options())
					options.Add(new FloatMenuOptionAndRefresh(NameFor(o), () => sel = o, this));

				foreach (int ex in ExtraOptions())
					options.Add(new FloatMenuOptionAndRefresh(NameForExtra(ex), () => extraOption = ex, this));

				DoFloatOptions(options);
			}
			return changed;
		}

		// Subclass can override DrawCustom to draw anything custom
		// (otherwise it's just label and option selection button)
		// Use either rect or WidgetRow in the implementation
		public virtual bool DrawCustom(Rect rect, WidgetRow row) => throw new NotImplementedException();

		// Auto detection of subclasses that use DrawCustom:
		private static readonly HashSet<Type> specialDrawers = null;
		private bool HasSpecial => specialDrawers?.Contains(GetType()) ?? false;
		static ListFilterDropDown()//<T>	//Remember there's a specialDrawers for each <T> but functionally that doesn't change anything
		{
			Type baseType = typeof(ListFilterDropDown<T>);
			foreach (Type subclass in baseType.AllSubclassesNonAbstract())
			{
				if (subclass.GetMethod(nameof(DrawCustom)).DeclaringType != baseType)
				{
					if (specialDrawers == null)
						specialDrawers = new HashSet<Type>();

					specialDrawers.Add(subclass);
				}
			}
		}
	}
}
