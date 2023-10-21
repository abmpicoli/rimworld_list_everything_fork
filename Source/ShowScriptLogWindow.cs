using Verse;
using UnityEngine;

namespace List_Everything
{
  public class ShowScriptLogWindow : EditWindow
  {
		private static Logger log = new Logger("ShowScriptLogWindow");
	public override void DoWindowContents(Rect inRect)
	{
			Vector2 scrollBarPosition = new();
			string s = Widgets.TextAreaScrollable(inRect.ScaledBy(0.9f), text, ref scrollBarPosition, true);
			log.log(()=>GetHashCode() + " ScriptLogWindow scrollBarPosition = " + scrollBarPosition + " text=" + s + ": event = " + Event.current);
			
	}

		public string text = "";

  }
}
