using System;
using System.Collections.Generic;
using Verse;

namespace List_Everything
{
	internal interface IScriptState
	{

		///
		/// true if the function can be invoked with the provided parameters.
		///
		bool CanInvoke(string function, params object[] args);

		/**
		 *	Invokes a script function with the provided input and delegates to the resultHandler the proper action regarding the output.
		 *	returns the result object. Will interrupt the engine if some error happens during the execution of the script.
		 *	If the function doesn't exists it will report an error and will interrupt the script if a non-existent function is called.
		 *	Check if the function can be called first.
		 *	
		 *	IsException can be used to check if an exception happened.
		 */
		object Invoke(string functionName, params object[] args);

		/**
			* true if the script parsing or function invocation raised an error, or if the caller did report an error.
			*/
		bool IsException();

		/**
		 * inform the script that an exception happened after the script was executed, which means the engine must be stopped for review.
		 * 
		 */
		void ReportError(Exception ex);

		/**
		 * Update the script name to a new value. If the value is changed, the engine should check if parsing is allowed and then
		 * reprocess the script.
		 */
		void UpdateScriptName(string sel);

		/**
		 * Gets the current script name.
		 */
		string GetScriptName();

		/**
		 * true if this instance is ready for receiving #invokeFunction calls.
		 */
		bool IsReady();

		/**
		 * change the readyness of the script from active to inactive or vice-versa. Used for on-off buttons.
		 */
		void PauseOrPlay();


		/**
		 * Try to Activate the script, which enable calls to InvokeFunction. If an issue happens (like a syntax error in the script,
		 * an error will be reported and the instance will be stopped.
		 */
		void Activate();
		/**
		 * true if debug mode is on: useful to allow scripts to log detailed messages, even if no error is raised.
		 * If Debugging is off, errors won't be raised at all.
		 */
		bool Debugging();


		/**
	* Toggles debug mode on or off. With debug mode on, simple log operations won't be reported. Used for on-off switches.
*/
		void DebugOnOff();

		/**
		 * Loads a script state based on a state definition.
		 */
		void Load(String stateDefinition);

		/**
		 * Saves a script state so a load can be used for it.
		 */
		string Save();

	}
}