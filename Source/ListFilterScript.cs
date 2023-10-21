using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Linq;
using System.Text;

namespace List_Everything
{

	public class ListFilterScript : ListFilter
	{
		private static ShowScriptLogWindow _showScriptLogWindow;
		private static ShowScriptLogWindow ScriptLogWindow
		{
			get
			{
				if (_showScriptLogWindow == null)
					_showScriptLogWindow = new ShowScriptLogWindow();
				return _showScriptLogWindow;
			}
		}
		private static Logger log = new Logger("ListFilterScript");

		public static readonly string namedLabel = "Script file:";
		public static float? namedLabelWidth;
		public static float NamedLabelWidth =>
			namedLabelWidth.HasValue ? namedLabelWidth.Value :
			(namedLabelWidth = Text.CalcSize(namedLabel).x).Value;

		public ListFilterScript()
		{
			log.log(() => id + "-" + "001-Constructor invoked");
		}

		public string ScriptData
		{
			get
			{
				return handler.Save();
			}
			set
			{
				handler.Load(value);
			}
		}

		public override void ExposeData()
		{

			log.log(() => id + "-" + "002-ExposeData called");
			base.ExposeData();
			string scriptData = handler.Save();
			Scribe_Values.Look(ref scriptData, "scriptspec");
			Scribe_Values.Look(ref timeoutSpec, "timeout");
			if (scriptData != handler.Save())
			{
				handler.Load(scriptData);
			}


		}

		public override ListFilter Clone()
		{
			log.log(() => "0036A-cloning " + id);
			ListFilterScript result = new ListFilterScript();
			result.handler.Load(handler.Save());
			result.timeoutSpec = this.timeoutSpec;
			log.log(() => "0036B - " + id + " cloned to " + result.id);
			result.currentSpec = handler.GetScriptName() + (timeoutSpec != 500 ? "|" + timeoutSpec : "");
			return result;

		}

		private IScriptState handler = new JavascriptScriptState();
		private DateTime expiration;
		private double timeoutSpec = 500.0;
		/** THe current filename specification. It can contain a '| timeout in ms now'
		 */
		private string currentSpec;

		public override IEnumerable<Thing> doApply(IEnumerable<Thing> original)
		{
			int c = original.Count();
			if (c == 0)
			{
				log.log(() => "empty list. returning it");
				return original;
			}

			log.log(() => "Evaluating expiration");
			expiration = DateTime.Now + TimeSpan.FromMilliseconds(timeoutSpec);
			log.log(() => "Starting evaluation. Expiration = " + expiration.ToString("HH.mm.ss.ffff") + " : current time is " + DateTime.Now.ToString("HH.mm.ss.ffff"));
			if (handler.IsReady() && handler.CanInvoke("doApply", original))
			{
				log.log(() => id + "-" + "003-invoking doApply in script");
				object result = null;
				try
				{
					log.log(() => id + " - 003A-Invoking doApply function");
					result = handler.Invoke(expiration - DateTime.Now, "doApply", original.ToList());
					
					if (handler.IsException() || result == null)
					{
						log.log(() => id + "-" + "004-null or exception. returning the original list");
						return base.doApply(original);
					}
					if (typeof(List<Thing>).IsAssignableFrom(result.GetType()))
					{
						List<Thing> toReturn = new();
						foreach (object o in ((List<Thing>)(result)))
						{
							log.log(() => id + "-" + "005-adding " + o);
							toReturn.Add((Thing)o);
						}

						return base.doApply(toReturn);
					} else
					{
						log.log(() => id + "-" + "006 - result of type " + result.GetType() + " is not a valid list value. Returning the original list");
						return base.doApply(original);
					}
				}
				catch (Exception ex)
				{

					handler.ReportError(() => id + "-" + "006-Exception raised: ", ex);
					return base.doApply(original);
				}
			}
			log.log(() => id + "-" + "007-no script. returning original list");
			return base.doApply(original);

		}

		protected override bool FilterApplies(Thing thing)
		{
			if (handler.IsReady() && expiration < DateTime.Now)
			{
				log.log(() => id + "-FA0131-A" + expiration.ToString("HHmmss.ffff") + " < " + DateTime.Now.ToString("HHmmss.ffff"));
				handler.ReportError(() => "timeout executing script", new Exception());
			}
			if (handler.IsReady() && handler.CanInvoke("doFilter", thing))
			{
				log.log(() => id + "-" + "FA0131-B-invoking filterapplies with " + thing);
				try
				{
					return (bool)handler.Invoke(expiration - DateTime.Now, "doFilter", thing);
				}
				catch (Exception ex)
				{
					Func<string> x = () => id + "FA0131-C-error applying filter";
					log.log(x, ex);
					handler.ReportError(x, ex);
					return true;
				}
			}
			return true;
		}

		public override bool DrawMain(Rect rect, bool locked)
		{
			log.log(() => "DRAWMAIN_START " + id + "-" + rect + " " + locked);
			try
			{
				Widgets.Label(rect, namedLabel);
				rect.xMin += NamedLabelWidth;
				GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}.STATE");
				Texture2D startTexture = TexButton.TurnedOff;
				log.log(() => id + "- 010-checking texture for start-stop");
				if (handler.IsReady())
				{
					startTexture = TexButton.TurnedOn;
				}
				bool switchState = false;
				Rect startStopPosition = new Rect(rect.RightPart(0.2f).position, new Vector2(startTexture.width, startTexture.height));
				log.log(() => id + "startstop position = " + startStopPosition);
				if (Widgets.ButtonImage(startStopPosition, startTexture))
				{
					log.log(() => id + "-011-button clicked");
					handler.PauseOrPlay();
					log.log(() => id + "-011B-Is ready = " + handler.IsReady());
					switchState = true;
				}
				GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}.LOG");
				Texture2D logTexture = TexButton.Log;
				if (handler.IsException())
				{

					log.log(() => id + "121-A - Texture is bad log due to exception");
					logTexture = TexButton.BadLog;
				}
				else if (handler.Debugging())
				{
					log.log(() => id + "121-B - Texture is logging active");
					logTexture = TexButton.Log;
				}
				else
				{
					log.log(() => id + "121-c - Texture is no logging");
					logTexture = TexButton.NoLog;
				}
				Rect logTexturePosition = new Rect(startStopPosition.RightPart(0).position, new Vector2(logTexture.width, logTexture.height));
				if (handler.IsException())
				{
					TooltipHandler.TipRegion(logTexturePosition, "script error: " + handler.ExceptionMessage());
				}
				if (Mouse.IsOver(logTexturePosition) && Event.current.shift)
				{
					string text = handler.LastExecutionLog;
					text = text + "\n";
					text = text + handler.PeekExecutionState() + "\n";
					text = text + handler.ExceptionMessage();


					if (ScriptLogWindow.IsOpen)
					{
						log.log(() => GetHashCode() + " - LFCDR0191 Window is open, checking refresh");
						if (ScriptLogWindow.text != text)
						{
							ScriptLogWindow.text = text;
							log.log(() => GetHashCode() + " - LFCDR0191A updating window");
							ScriptLogWindow.WindowUpdate();
						}
					}
					else
					{
						ScriptLogWindow.text = text;
						Find.WindowStack.Add(ScriptLogWindow);
					}

				}

				if (Widgets.ButtonImage(logTexturePosition, logTexture))
				{
					log.log(() => id + "012-button clicked");
					handler.DebugOnOff();


					log.log(() => id + "012-new debug mode=" + handler.Debugging());
					switchState = true;
				}
				if (GUI.GetNameOfFocusedControl() == $"LISTFILTERPYTHONINPUT{id}" &&
					Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					GUI.FocusControl("");
					Event.current.Use();
				}

				GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}");
				if (locked)
				{
					log.log(() => id + "013-script is locked");
					Widgets.Label(rect.LeftPart(0.8f), handler.GetScriptName());
				}
				else
				{
					string newStr = Widgets.TextField(rect.LeftPart(0.8f), currentSpec);
					log.log(() => id + "-013B checking text field - value = " + newStr);

					if (newStr != currentSpec)
					{
						log.log(() => id + "013-script = " + handler.GetScriptName() + " timeout = " + timeoutSpec);
						currentSpec = newStr;
						handler.UpdateScriptName(ScriptFromSpec(newStr));
						timeoutSpec = TimeoutFromSpec(newStr);
						switchState = true;
					}
				}

				if (Widgets.ButtonImage(rect.RightPartPixels(rect.height), TexUI.RotLeftTex))
				{
					GUI.FocusControl("");

					return true;
				}
				log.log(() => id + "-" + "final swithstate = " + switchState);
				return switchState;
			}
			finally
			{
				log.log(() => "DRAWMAIN_END");
			}
		}

		private string ScriptFromSpec(string newStr)
		{
			int x = newStr.IndexOf('|');
			if (x >= 0)
			{
				if (x == 0)
				{
					return "";
				}
				return newStr.Substring(0, x);
			}
			return newStr;
		}

		private double TimeoutFromSpec(string newStr)
		{
			int x = newStr.IndexOf('|');
			if (x > 0)
			{
				try
				{
					double timeout = double.Parse((newStr.Substring(x + 1).Trim()));
					log.log(() => "Timeout parsed as " + timeout);
					return timeout;
				}
				catch (Exception ex)
				{
					log.log(() => "exception parsing timeout. Set to 500 " + ex.Message);
					return 500;
				}

			}
			log.log(() => "using default 500ms timeout");
			return 500;

		}

		protected override void DoFocus()
		{
			GUI.FocusControl($"LISTFILTERPYTHONINPUT{id}");
		}

	}
}
