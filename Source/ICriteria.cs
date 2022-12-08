using UnityEngine;
using Verse;

namespace List_Everything
{
	/**
	 * Represents a single instance of a thing that evaluates a FindAlertData situation against a specific criteria.
	 */
	public interface ICriteria
	{
		/**
		 * Evaluates if the provided data fits the provided criteria;
		 * true if the data fits the criteria, false otherwise.
		 */
		public bool Fits();


		/**
		 * How to draw this specification at the alerts screen
		 */
		void DrawSpecificationDetails(WidgetRow row);

		/**
		 * get a representation of the metric used for this criteria.
		 */
		public string Metric
		{
			get;
		}

		/**
		 * Get/Set a representation of the desired target.
		 */
		public string Target
		{
			get; set;
		}

		/**
		 * A reference to the factory used to generate the criteria;
		 */
		public ICriteriaFactory Factory { get; }
		public FindAlertData Data { get; set; }
	}
}
