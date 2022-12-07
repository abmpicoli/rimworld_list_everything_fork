using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System;

namespace List_Everything
{
	

	public class Alert_Find_Cache
	{
		private int count;
		private IEnumerable<Thing> foundThings;
		int Count
		{
			get
			{
				Recalculate();
				return count;
			}
		}

		private void Recalculate()
		{
			throw new NotImplementedException();
		}

		IEnumerable<Thing> FoundThings
		{
			get
			{
				Recalculate();
				return foundThings;
			}
		}

	}


	public class Alert_Find : Alert
	{
		private static Logger log = new Logger("Alert_Find");

		private FindAlertData _alertData;
		public FindAlertData AlertData
		{
			get
			{
				return _alertData;
			}
			set
			{
				defaultLabel = value.Name;
				defaultPriority = value.alertPriority;
				_alertData = value;
			}
		}

		public int maxItems = 16;
		int tickStarted;

		public static bool enableAll = true;

		public Alert_Find()
		{
			log.log(() => GetHashCode() + ":Created new AlertFind ");
		}

		public Alert_Find(FindAlertData d) : this()
		{
			log.log(() => "Created new AlertFind with " + d.Name);
			AlertData = d;
		}

		//copied from Alert_Critical
		private const float PulseFreq = 0.5f;
		private const float PulseAmpCritical = 0.6f;

		private static readonly Color standardColor = new Color(0.9f, 0.9f, 0.9f, 0.2f);

		//protected but using publicized assembly
		//protected override Color BGColor
		protected override Color BGColor
		{
			get
			{
				Color result;

				if (defaultPriority != AlertPriority.Critical)
				{
					result = standardColor;
				}
				else
				{
					float i = Pulser.PulseBrightness(PulseFreq, Pulser.PulseBrightness(PulseFreq, PulseAmpCritical));
					result = new Color(i, i, i) * Color.red;
				}
				log.log(() => this.GetHashCode() + this?.AlertData?.Name + ": bgcolor set to " + result);
				return result;
			}
		}

		public string Name
		{
			get
			{
				return AlertData != null ? AlertData.Name : "#undefined";
			}

		}

		public override string GetLabel()
		{
			string result;
			if (AlertData != null)
			{
				result = AlertData.Name + ":" + FoundThings().Sum(t => t.stackCount);
			}
			else
			{
				result = "???";
			}
			log.log(() => this.GetHashCode() + " " + AlertData?.Name + ": label set to " + result);
			return result;
		}




		public override AlertReport GetReport()
		{
			log.log(() => GetHashCode() + " : Getting AlertReport: alertData = " + AlertData + " : enableAll = " + enableAll);
			if (AlertData == null || !enableAll)  //Alert_Find auto-added as an Alert subclass, exists but never displays anything
				return AlertReport.Inactive;

			var things = FoundThings();
			int count = things.Sum(t => t.stackCount);
			log.log(() => GetHashCode() + ": count = " + count);
			bool active = AlertData != null ? AlertData.Evaluate() : false;
			
			
			log.log(() => GetHashCode() + " tickStarted = " + tickStarted + " ; ticksGame = " + Find.TickManager.TicksGame + "; to show alert = " + AlertData.ticksToShowAlert);
			if (!active)
			{
				log.log(() => GetHashCode() + " : inactive. ");
				tickStarted = Find.TickManager.TicksGame;
			}
			else if (Find.TickManager.TicksGame - tickStarted >= AlertData.ticksToShowAlert)
			{

				if (count == 0)
					return AlertReport.Active;
				return AlertReport.CulpritsAre(things.Take(maxItems).ToList());
			}
			return AlertReport.Inactive;
		}

		public override TaggedString GetExplanation()
		{
			var things = FoundThings();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(defaultLabel + AlertData.desc.mapLabel);
			stringBuilder.AppendLine(" - " + MainTabWindow_List.LabelCountThings(things));
			stringBuilder.AppendLine("");
			foreach (Thing thing in things.Take(maxItems))
				stringBuilder.AppendLine("   " + thing.Label);
			if (things.Count() > maxItems)
				stringBuilder.AppendLine("TD.Maximum0Displayed".Translate(maxItems));
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine("TD.Right-clickToOpenFindTab".Translate());

			return stringBuilder.ToString().TrimEndNewlines();
		}

		int currentTick;
		

		public override Rect DrawAt(float topY, bool minimized)
		{
			Text.Font = GameFont.Small;
			string label = this.GetLabel();
			float height = Text.CalcHeight(label, Width - 6); //Alert.TextWidth = 148f
			Rect rect = new Rect((float)UI.screenWidth - Width, topY, Width, height);
			//if (this.alertBounce != null)
			//rect.x -= this.alertBounce.CalculateHorizontalOffset();
			if (Event.current.button == 1 && Widgets.ButtonInvisible(rect, false))
			{
				MainTabWindow_List.OpenWith(AlertData.desc.Clone(AlertData.desc.map), true);

				Event.current.Use();
			}
			return base.DrawAt(topY, minimized);
		}
	}
}
