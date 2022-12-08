using System;
using System.Collections.Generic;

namespace List_Everything
{
	public class CriteriaFactory : ICriteriaFactory
	{
		private static Logger log = new Logger("CriteriaFactory");
		private string _identifier;
		private UnityEngine.Texture2D symbol;
		private Func<FindAlertData, ICriteria, bool> evaluator;
		private string caption;
		private static readonly Dictionary<string, ICriteriaFactory> knownTypes = new();

		public string Identifier => _identifier;


		/**
	* adds a new criteria specification to the factory.
*/
		public static void Add(ICriteriaFactory criteria)
		{
			if (knownTypes.ContainsKey(criteria.Identifier))
			{
				log.fatal(() => "Warning: there is already a criteria with the identifier " + criteria.Identifier + ". Bypassing it");
				return;
			}
			knownTypes.Add(criteria.Identifier, criteria);


		}



		static CriteriaFactory()
		{
			Add(new CriteriaFactory("greater_than", (data, criteria) => data.CurrentState.Count > Parse(criteria.Metric, int.MaxValue), TexButton.GreaterThan, ">"));
			Add(new CriteriaFactory("less_than", (data, criteria) => data.CurrentState.Count < Parse(criteria.Metric, int.MinValue), TexButton.LessThan, "<"));
			Add(new CriteriaFactory("exists", (data, criteria) => data.CurrentState.Things.Count > 0, TexButton.Equals, " found "));
		}

		/**
		 * Parse a value as integer, returning the default value if the metric is not parseable.
		 */
		private static int Parse(string metric, int defaultValue)
		{
			int outValue;
			if (int.TryParse(metric, out outValue))
			{
				return outValue;
			}
			return defaultValue;
		}

	public ICriteria NewInstance(FindAlertData data, string target)
	{
			return new CompareType(this, this.evaluator, this.symbol, target, caption);
	}

	public CriteriaFactory(string key, Func<FindAlertData, ICriteria, bool> evaluator, UnityEngine.Texture2D symbol, string caption)
		{
			_identifier = key;
			this.symbol = symbol;
			this.evaluator = evaluator;
			this.caption = caption;
		}

	/**
	 * Gets a criteria that is "just next" in the criteria order according to the specification.
	 */
	public static ICriteria Roll(ICriteria countComp)
	{
			string current = countComp.Factory.Identifier;
			bool foundMe = false;
	  foreach (ICriteriaFactory x in knownTypes.Values)
		 {
				if(foundMe)
				{
					ICriteria newCriteria = x.NewInstance(countComp.Data, countComp.Target);
				}
				if(x.Equals(current))
				{
					foundMe = true;
				}
		 }
	  throw new NotImplementedException();
	}
  }
}