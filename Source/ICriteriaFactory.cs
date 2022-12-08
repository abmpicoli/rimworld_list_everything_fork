namespace List_Everything
{
	/**
	 * Represents a comparison option to be placed.
	 */
  public interface ICriteriaFactory
  {
		/**
		 * Gets the identity of this criteria factory. Used on saves and to switch criteria types.
		 */
	string Identifier { get; }

	/**
	 * Instantiates a new comparison object based on the provided criteria and the data provided.
	 */
	public ICriteria NewInstance(FindAlertData data,string target);

  }
}