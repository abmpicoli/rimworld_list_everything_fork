﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;

namespace List_Everything
{
	[StaticConstructorOnStartup]
	public static class ContentsUtility
	{
		private static FieldInfo contentsKnownInfo = typeof(Building_Casket).GetField("contentsKnown", BindingFlags.NonPublic | BindingFlags.Instance);
		public static bool get_contentsKnown(this Building_Casket building) =>
			(bool)contentsKnownInfo.GetValue(building);

		public static bool CanPeekInventory(this IThingHolder holder) =>
			DebugSettings.godMode ||
			(holder is Building_Casket c ? c.get_contentsKnown() : true);
	}
}