using System.Collections.Generic;
using Verse;
using RimWorld;

namespace List_Everything
{
  public class AlertCache
	{
		private static Logger log = new Logger("AlertCache");
		private List<Alert> allAlerts;
		private List<Alert> activeAlerts;

		private void PopulateLists()
		{
			
			if (allAlerts == null || activeAlerts == null)
			{
				log.log(()=>GetHashCode() + ": getting list by reflection");
				AlertsReadout x = (Find.UIRoot as UIRoot_Play)?.alerts as AlertsReadout;
				activeAlerts = ReflectionUtils.fieldValue(x, "activeAlerts", (List<Alert>)null);
				allAlerts = ReflectionUtils.fieldValue(x, "AllAlerts", (List<Alert>)null);

			}

		}

		private List<Alert> AllAlerts
		{
			get
			{
				PopulateLists();
				return allAlerts;
			}
		}

		private List<Alert> ActiveAlerts
		{
			get
			{
				PopulateLists();
				return activeAlerts;
			}

		}

		private Game game;

		public AlertCache(Game game)
		{
			this.game = game;
		}
		public Game Game { get { return game; } }

		public override string ToString()
		{
			return "AlertCache:" + GetHashCode() + " - " + Game?.GetHashCode();
		}

		public Alert_Find FindAlert(string name)
		{
			if(name == null)
			{
				return null;
			}
			PopulateLists();
			List<Alert> a = AllAlerts;
			log.log(() => GetHashCode() + " allAlerts = " + a);
			return (Alert_Find)a?.Find((s) => {
				if (s == null)
				{
					log.log(() => GetHashCode() + " item is null. not a match");
					return false;
				}
				log.log(() => GetHashCode() + ": type is " + s.GetType().FullName);
				if (!typeof(Alert_Find).IsAssignableFrom(s?.GetType())) {
					log.log(() => GetHashCode() + ": not a type match. Bypassing");
					return false;
				}
				string label = ((Alert_Find)s).Name;
				log.log(() => "Label is " + label);
				if (label == null)
				{
					log.log(() => GetHashCode() + ": label is null. Bypassing");
					return false;
				}
				return name.Equals(label);
			});
		}

		public void Remove(string name)
		{
			if (name != null)
			{
				Alert_Find s = FindAlert(name);
				if (s != null)
				{
					AllAlerts?.Remove(s);
					ActiveAlerts?.Remove(s);
				}
			}

		}



		public void Activate(FindAlertData findAlertData)
		{
			Alert_Find alert = FindAlert(findAlertData.Name);
			if (alert == null)
			{
				Alert_Find newAlert = new Alert_Find(findAlertData);
				AllAlerts?.Add(newAlert);
				ActiveAlerts?.Add(newAlert);
			} else
			{
				alert.AlertData = findAlertData;
			}
			
		}
	}
}
