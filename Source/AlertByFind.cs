using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace List_Everything
{
	public static class AlertByFind
	{
		private static Logger log = new Logger("AlertByFind");
		public static void AddAlert(FindAlertData alert, bool overwrite = false, Action okAction = null)
		{
			log.log(() => "AddAlert " + alert.GetHashCode() + " , " + overwrite + " , " + okAction + " invoked");

			if (!overwrite && GetAlert(alert.desc.name) != null)
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
					"TD.OverwriteAlert".Translate(), () =>
					{
						RemoveAlert(alert.desc.name);
						AddAlert(alert, true, okAction);
						FindAlertData.Activate(alert);

					}));
			}
			else
			{
				FindAlertData.Activate(alert);
				okAction?.Invoke();
			}
		}

		public static FindAlertData GetAlert(string name)
		{
			return FindAlertData.GetAlertData(name);
		}

		public static void RemoveAlert(string name)
		{
			FindAlertData.RemoveAlert(name);

		}

		public static void RenameAlert(string name, string newName, bool overwrite = false, Action okAction = null)
		{
			if (!overwrite && GetAlert(newName) != null)
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
					"TD.OverwriteAlert".Translate(), () =>
					{
						RemoveAlert(newName);
						RenameAlert(name, newName, true, okAction);
					}));
			}
			else
			{
				okAction?.Invoke();
				GetAlert(name)?.Rename(newName);
			}
		}

		public static void SetPriority(string name, AlertPriority p) =>
			GetAlert(name)?.SetPriority(p);

		public static void SetTicks(string name, int t) =>
			GetAlert(name)?.SetTicks(t);
		}
}
