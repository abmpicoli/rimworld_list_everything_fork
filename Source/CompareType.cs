using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace List_Everything
{
	public class CompareType : ICriteria
	{
		private ICriteriaFactory factory;
		private Func<FindAlertData, string> metricFunction;
		private FindAlertData alertData;
		private string _target;
		private Texture2D icon;
		private string caption;

		/**
		 * the evaluator receives a metric and a target, and then tells if the metric fits the target.
		 */
		private Func<string, string, bool> evaluator;

		public CompareType(ICriteriaFactory factory, FindAlertData data, Func<FindAlertData, string> metric, Func<string, string, bool> evaluator, Texture2D symbol, string initialTarget, string caption)
		{
			this.factory = factory;
			metricFunction = metric;
			alertData = data;
			_target = initialTarget;
			this.icon = symbol;
			this.caption = caption;
			this.evaluator = evaluator;
		}

		public string Metric { get => metricFunction(alertData); }
		public string Target { get => _target; set => _target = value; }

		public ICriteriaFactory Factory => factory;

	public FindAlertData Data { get => alertData; set => alertData=value; }

	public void DrawSpecificationDetails(WidgetRow row)
		{
			if(row.ButtonIcon(this.icon))
			{
				
			}

			row.
		}

		public bool Fits()
		{
			return evaluator(Metric, Target);
		}
	}
}