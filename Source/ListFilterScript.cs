using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Linq;

namespace List_Everything
{
	
  public class ListFilterScript: ListFilter
	{
		private static Logger log = new Logger("ListFilterScript");
		
		public static readonly string namedLabel = "Script file:";
		public static float? namedLabelWidth;
		public static float NamedLabelWidth =>
			namedLabelWidth.HasValue ? namedLabelWidth.Value :
			(namedLabelWidth = Text.CalcSize(namedLabel).x).Value;

		public ListFilterScript()
		{
			log.log(()=>id+"-"+"001-Constructor invoked");
		}

	public string ScriptData
	{
	  get {
				return handler.Save();
				}
	  set { 
				handler.Load(value);
		}
	}

	public override void ExposeData()
	{

			log.log(()=>id+"-"+"002-ExposeData called");
			base.ExposeData();
			string scriptData = handler.Save();
			Scribe_Values.Look(ref scriptData, "scriptspec");
			if(scriptData != handler.Save())
			{
				handler.Load(scriptData);
			}


	}

	public override ListFilter Clone()
	{
			log.log(()=>"0036A-cloning " + id);
			ListFilterScript result = new ListFilterScript();
			result.handler.Load(handler.Save());
			log.log(()=>"0036B - "+ id + " cloned to " + result.id);
			return result;

	}

	private IScriptState handler = new JavascriptScriptState();

		public override IEnumerable<Thing> doApply(IEnumerable<Thing> original)
		{
			if (handler.IsReady() && handler.CanInvoke("doApply",original))
			{
				log.log(()=>id+"-"+"003-invoking doApply in script");
				IEnumerable<object> result = null;
				try
				{

					result = (List<object>) handler.Invoke("doApply", original.ToList());
					if (handler.IsException() || result == null)
					{
						log.log(()=>id+"-"+"004-null or exception. returning the original list");
						return original;
					}
					else
					{
						List<Thing> toReturn = new();
						foreach (object o in result)
						{
							log.log(()=>id+"-"+"005-adding " + o);
							toReturn.Add((Thing)o);
						}
						
						return toReturn;
					}
				}
				catch (Exception ex)
				{
					log.log(()=>id+"-"+"006-Exception raised: ", ex);
					handler.ReportError(ex);
					return original;
				}
			}
			log.log(()=>id+"-"+"007-no script. returning original list");
			return base.doApply(original);

		}

		protected override bool FilterApplies(Thing thing)
		{
			
			if (handler.IsReady() && handler.CanInvoke("doFilter", thing))
			{
				log.log(()=>id+"-"+"008-invoking filterapplies with " + thing);
				try
				{
					return (bool)handler.Invoke("doFilter",thing);
				} catch(Exception ex)
				{
					log.log(()=>id+"009-error:", ex);
					handler.ReportError(ex);
					return true;
				}
			}
			return true;
		}

		public override bool DrawMain(Rect rect, bool locked)
		{
			log.log(()=>id+"-"+"010-starting drawMain " + rect + " " + locked);
			Widgets.Label(rect, namedLabel);
			rect.xMin += NamedLabelWidth;
			GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}.STATE");
			Texture2D startTexture = TexButton.TurnedOff;
			log.log(()=>id+"- 010-checking texture for start-stop");
			if (handler.IsReady())
			{
				startTexture = TexButton.TurnedOn;
			}
			bool switchState = false;
			Rect startStopPosition = new Rect(rect.RightPart(0.2f).position, new Vector2(startTexture.width,startTexture.height));
			log.log(()=>id+"startstop position = " + startStopPosition);
			if (Widgets.ButtonImage(startStopPosition,startTexture))
			{
				log.log(()=>id+"-011-button clicked");
				handler.PauseOrPlay();
				log.log(()=>id+"-011B-Is ready = " + handler.IsReady());
				switchState = true;
			}
			GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}.LOG");
			Texture2D logTexture = TexButton.Log;
			if(handler.IsException())
			{
				log.log(()=>id+"121-A - Texture is bad log due to exception");
				logTexture = TexButton.BadLog;
			} else if (handler.Debugging())
			{
				log.log(()=>id+"121-B - Texture is logging active");
				logTexture = TexButton.Log;
			} else
			{
				log.log(()=>id+"121-c - Texture is no logging");
				logTexture = TexButton.NoLog;
			}
			Rect logTexturePosition = new Rect(startStopPosition.RightPart(0).position, new Vector2(logTexture.width, logTexture.height));
			
			if(Widgets.ButtonImage(logTexturePosition, logTexture)) {
				log.log(()=>id+"012-button clicked");
				handler.DebugOnOff();
				
				
				log.log(()=>id+"012-new debug mode=" + handler.Debugging());
				switchState = true;
			}
			if (GUI.GetNameOfFocusedControl() == $"LISTFILTERPYTHONINPUT{id}" &&
				Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
			{
				GUI.FocusControl("");
				Event.current.Use();
			}

			GUI.SetNextControlName($"LISTFILTERPYTHONINPUT{id}");
			if (locked) {
				log.log(()=>id+"013-script is locked");
				Widgets.Label(rect.LeftPart(0.8f), handler.GetScriptName());
			} else {
				string newStr = Widgets.TextField(rect.LeftPart(0.8f), handler.GetScriptName());
				log.log(()=>id + "-013B checking text field - value = " + newStr);
				if (newStr != handler.GetScriptName())
				{
					log.log(()=>id+"013-script = " + handler.GetScriptName());
					handler.UpdateScriptName(newStr);
					switchState = true;
				}
			}

			if (Widgets.ButtonImage(rect.RightPartPixels(rect.height), TexUI.RotLeftTex))
			{
				GUI.FocusControl("");
				handler.UpdateScriptName("");
				return true;
			}
			log.log(()=>id+"-"+"final swithstate = " + switchState);
			return switchState;
		}

		protected override void DoFocus()
		{
			GUI.FocusControl($"LISTFILTERPYTHONINPUT{id}");
		}

	}
}
