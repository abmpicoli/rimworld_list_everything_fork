using System;
using Verse;

namespace List_Everything
{
	public class JavascriptScriptState : IScriptState
	{
		private Logger log;

		private Logger GetLogger()
		{
			if (log == null)
			{
				log = new Logger("script-" + GetScriptName()+GetHashCode());
			}
			return log;
		}
		public void Activate()
		{

		}

		public bool CanInvoke(string function, params object[] args)
		{
			return false;
		}
		private bool debugMode;

	public string GetLogLocation()
	{
	  if(log != null)
	  {
				return log.GetLogFile();
	  }
			return ("Not defined!");
	}

	public bool Debugging()
		{
			return debugMode;
		}

		public void DebugOnOff()
		{
			debugMode = !debugMode;
		}

		public object Invoke(string functionName, params object[] args)
		{
			throw new NotImplementedException();
		}

		public bool IsException()
		{
			return false;
		}

		public bool IsReady()
		{
			return !paused;
		}
		bool paused = true;
		public void PauseOrPlay()
		{
			paused = !paused;
		}

		public void ReportError(Exception ex)
		{
			return;
		}
		string scriptName = "";
		public void UpdateScriptName(string sel)
		{
			scriptName = sel;
		}

		public string GetScriptName()
		{
			return scriptName;
		}

		public void Load(string stateDefinition)
		{
			
			string[] values = scriptName.Split(new string[] { "///" }, StringSplitOptions.None);
			try
			{
				scriptName = values[0];
				debugMode = bool.Parse(values[1]);
				paused = bool.Parse(values[2]);
			}
			catch { 

				
			} finally
			{
				GetLogger().log(()=>GetHashCode()+":0088-A Load invoked with definition " + stateDefinition);
				GetLogger().log(()=>GetHashCode()+":0088-B Our state is now " + Save());
				GetLogger().log(()=>GetHashCode()+":0088-C values read :" + string.Join("  ,  ", values));
			}
		}

		public string Save()
		{
			string result = scriptName + "///" + debugMode + "///" + paused;
			GetLogger().log(()=>GetHashCode()+":0097-A Save invoked. Persisted value = " + result);
			return result;
		}
	}

}