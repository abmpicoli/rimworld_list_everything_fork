using UnityEngine;

namespace List_Everything
{
	/**
	 * Represents a single instance of a thing that evaluates a FindAlertData situation against a specific criteria.
	 */
  public interface ICompareType
  {
		/**
		 * 
		 */
		public bool Evaluate();
		public Texture2D Icon();
  }
}
