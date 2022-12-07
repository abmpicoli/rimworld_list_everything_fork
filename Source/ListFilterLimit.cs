using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;


namespace List_Everything
{
  class ListFilterLimit : ListFilterWithOption<string>
	{
		public ListFilterLimit() => sel = "10";

		protected override bool FilterApplies(Thing thing)
		{
			return true;
		}
		public override IEnumerable<Thing> doApply(IEnumerable<Thing> list)
		{
	  try
	  {

				return list.Take(Int32.Parse(sel.Trim()));

			}
			catch (Exception)
			{
				return list;
			}
		}




		public static readonly string namedLabel = "Limit to (#): ";
		public static float? namedLabelWidth;
		public static float NamedLabelWidth =>
			namedLabelWidth.HasValue ? namedLabelWidth.Value :
			(namedLabelWidth = Text.CalcSize(namedLabel).x).Value;

		public override bool DrawMain(Rect rect, bool locked)
		{
			Widgets.Label(rect, namedLabel);
			rect.xMin += NamedLabelWidth;

			if (locked)
			{
				Widgets.Label(rect, '"' + sel + '"');
				return false;
			}

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
				sel = "10";
				return true;
			}
			return false;
		}
	}
}
