using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using System;

namespace List_Everything
{
	public class Mod : Verse.Mod
	{
		private static Logger log = new Logger("Mod");
		private static string _modId;
		private static string _modName;
		public static Settings settings;
		public Mod(ModContentPack content) : base(content)
		{
			_modId = content.ModMetaData.PackageId;
			_modName = content.ModMetaData.Name;
			LongEventHandler.ExecuteWhenFinished(() => { settings = GetSettings<Settings>(); });
			log.log(() =>
			{
				IScriptState state = new JavascriptScriptState();
				state.Script = "function helloWorld(x) { return \"Hello world with \"+ x ; }";
				state.Activate(TimeSpan.FromMilliseconds(100));
				return state.Invoke(TimeSpan.FromMilliseconds(100),"helloWorld", "YAY!");
			});
			
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "TD.ListEverything".Translate();
		}
		public static string ModId
		{
			get => _modId;
		}
		public static string Name { get => _modName; }

	}
	
}