namespace List_Everything
{
	/**
	 * Represents a comparison option to be placed.
	 */
  public interface ICompareTypeFactory
  {

		/**
		 * Instantiates a new comparison object based on the provided criteria and the data provided.
		 */
		public ICompareType NewInstance(FindAlertData data,object criteria);

  }
}