using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace List_Everything
{


  public class FindAlertData : IExposable
	{
		private static AlertCache _alertCache = null;
		public static AlertCache AlertCache
		{
			get
			{
				if (_alertCache == null || _alertCache?.Game != Verse.Current.Game)
				{
					log.log(()=>"_alertCache = " + _alertCache + " and game = " + Verse.Current.Game?.GetHashCode() + "; creating a new cache");
					_alertCache = new AlertCache(Verse.Current.Game);
				}
				return _alertCache;
			}
		}
		private static Logger log = new("FindAlertData");

		public FindDescription desc;

		public AlertPriority alertPriority;
		public int ticksToShowAlert;
		public int countToAlert;
		public ICriteria countComp;

	public override string ToString()
	{
			return "FindAlertData " + GetHashCode() + ": " + this.desc + "; count=" + this.count + "; countToAlert=" + this.countToAlert + "; compareTime=" + this.countComp;
	}

	public static FindAlertData GetAlertData(string name)
		{
			log.log(() => "GetAlertData " + name + " invoked");
			FindAlertData data = AlertCache.FindAlert(name)?.AlertData;
			log.log(() => "AlertData " + name + " = " + data);
			return data;
		}

		public static void RemoveAlert(string name)
		{
			AlertCache.Remove(name);

		}

		public FindAlertData()
		{
			log.log(()=>GetHashCode() + " find alert data created");
		}

		public FindAlertData(FindDescription d)
		{
			log.log(()=>GetHashCode() + " find alert data created for "+d);
			desc = d;
		}

		public Map _scribeMap;
		private string _name;
		public string Name
		{
			get
			{
				if (_name != null)
				{
					return _name;
				}
				_name = desc.name;
				return _name;
			}
			set
			{
				if (AlertCache.FindAlert(value) != null)
				{
					throw new Exception("Invalid state: there is already an alert with this name");
				}
				_name = value;
			}
		}

	public FindStateCache CurrentState { get; internal set; }

	private ICriteria compareType;
		private int count;

		public void ExposeData()
		{

			Scribe_Deep.Look(ref desc, "desc");
			Scribe_Values.Look(ref alertPriority, "alertPriority");
			Scribe_Values.Look(ref ticksToShowAlert, "ticksToShowAlert");
			Scribe_Values.Look(ref countToAlert, "countToAlert");
			Scribe_Values.Look(ref countComp, "countComp");

			if (Scribe.mode == LoadSaveMode.Saving)
				_scribeMap = desc.map;

			Scribe_References.Look(ref _scribeMap, "map");

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				desc.map = _scribeMap;
				_scribeMap = null;
			}
		}

		public void Rename(string newName)
		{

			Alert_Find x = AlertCache.FindAlert(Name);
			if (x == null)
			{
				Name = newName;
				AlertCache.Activate(this);
			} else
			{
				x.AlertData = this;

			}
		}

		private int currentTick = -1;

		private IEnumerable<Thing> FoundThings()
		{
			int gameTick = Find.TickManager.TicksGame * 10 / ticksToShowAlert;
			if (gameTick == currentTick)
				return desc.ListedThings;

			currentTick = gameTick;

			desc.RemakeList();
			return desc.ListedThings;
		}

		public void SetPriority(AlertPriority p)
		{
			this.alertPriority = p;
		}

		public void SetTicks(int t)
		{
			this.ticksToShowAlert = t;
		}

	public bool Evaluate()
	{
	  throw new NotImplementedException();
	}

	public void SetCount(int c)
		{
			this.count = c;
		}

		public void SetComp(ICriteria c)
		{
			this.compareType = c;
		}

		public static void Activate(FindAlertData alert)
		{
			AlertCache.Activate(alert);
		}
		public void Activate()
		{
			AlertCache.Activate(this);
		}
	}
}
