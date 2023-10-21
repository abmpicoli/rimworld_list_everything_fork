using Jint;
using Jint.Native;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace List_Everything
{
	public class JavascriptScriptState : IScriptState
	{
		private static Logger classLogger = new Logger("JavascriptScriptState");
		private bool active = false;
		private Logger log;
		private string logName = null;

		private Logger GetLogger()
		{
			if(!active)
			{
				logName = null;
				if(log != null)
				{
					log.log(() => "returning logging to javascriptScriptState");
				}
				log = null;
				return classLogger;
			}
			if(log == null || logName != scriptName)
			{
				string newName = "script-" + scriptName + "." + DateTime.Now.ToString("HHmmss.ffff") + ".h-" + this.GetHashCode();
				if (log != null)
				{
					log.log(() => "log to " + newName + " started");
				}
				log = new Logger(newName);
				logName = scriptName;
				return log;
			}
			return log;

		}



		public void Activate(TimeSpan timeout)
		{
			active = true;
			isException = false;
			Logger thisLog = GetLogger();

			thisLog.StartCapture();
			thisLog.log(() => "Starting script parsing");
			RunWithTimeout(timeout, () =>
			 {
				 try
				 {
					 string candidateSource = ReadScript(scriptName);
					 if (candidateSource == null)
					 {
						 candidateSource = ReadScript(Logger.GetRootDirectory() + Path.DirectorySeparatorChar + scriptName);
					 }
					 if (candidateSource == null)
					 {
						 thisLog.log(() => "Script " + scriptName + " not found. ");
						 candidateSource = Script;
					 }

					 if (candidateSource == null)
					 {
						 thisLog.fatal(() => "No script found to execute");
						 paused = true;
						 debugMode = true;
						 isException = true;
						 ReportError(() => scriptName + " not found", new Exception(scriptName + " not found"));
						 return;
					 }
					 thisLog.log(() => "===== applying script " + scriptName + ":\n=====\n" + candidateSource + "\n= = =");
					 scriptSource = candidateSource;
					 engine = new Engine(cfg =>
					 {
						 cfg.AllowClr();
						 cfg.AllowClr(this.GetType().Assembly);
						 cfg.AllowClr(typeof(Verse.Game).Assembly);
					 });
					 try
					 {
						 Action<object> inspect = (o) =>
						 {
							 ReflectionUtils.Inspect(GetLogger(), o);
						 };
						 Action<object> log = (o) =>
						 {
							 GetLogger().log(() => o);
						 };
						 Func<object, JavascriptWrapperHack> wrap = (o) => (new JavascriptWrapperHack(o, thisLog));
						 engine.SetValue("inspect", inspect);
						 engine.SetValue("log", log);
						 engine.SetValue("wrap", wrap);
						 engine.Execute(scriptSource);
						 thisLog.log(() => "Script loaded");
					 }
					 catch (Exception e)
					 {
						 ReportError(() => "Parse error when executing the script: " + e.Message, e);
					 }

				 }
				 finally
				 {
					 lastExecutionLog.Clear();
					 lastExecutionLog.Append(thisLog.EndCapture());

				 }
			 });
			 

		}



		private bool RunWithTimeout(TimeSpan timeSpan, Action p)
		{
			if (timeSpan <= TimeSpan.Zero)
			{
				ReportError(() => "Timeout running action. Timespan Negative ", new TimeoutException());
			}
			GetLogger().log(() => "Action starting");
			Task x = new(p, new CancellationTokenSource().Token);
			x.Start();
			x.Wait();
			if (x.IsFaulted)
			{
				GetLogger().log(() => "Action finished with fault");
				ReportError(() => "Exception running action", x.Exception);
			}
			if (!x.IsCompleted)
			{
				GetLogger().log(() => "Action not finished. Interrupted");
				ReportError(() => "Timeout running action", x.Exception);
			}
			return x.IsCompleted;


		}

		/**
		 * Reads a file. Returns null if the file doesn't exists.
		 */
		private string ReadScript(string fileName)
		{
			if (File.Exists(fileName))
			{
				return File.ReadAllText(fileName);
			}
			GetLogger().log(() => Path.GetFullPath("" + fileName) + "not found");
			return null;
		}

		public bool CanInvoke(string function, params object[] args)
		{
			if (engine == null)
			{
				return false;
			}

			Jint.Native.JsValue x = engine.GetValue(function);
			bool result = x != null && x != Jint.Native.JsValue.Undefined; 
			log.log(() => "Can invoke " + function + " = " + x + " : " + result);
			return result;
		}
		private bool debugMode;

		public string GetLogLocation()
		{
			if (log != null)
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


		public object Invoke(TimeSpan timeout, string functionName, params object[] args)
		{
			lastExecutionLog.Clear();
			thisLogger = GetLogger();
			if (debugMode)
			{
				thisLogger.StartCapture();
			}
			thisLogger.log(() => "IV001A - Invoking (" + functionName + " with timeout " + timeout + ")");
			object result = null;
			if (!RunWithTimeout(timeout, () =>
			 {
				 try
				 {
					 var x = engine.GetValue(functionName);
					 JsValue[] jars = new JsValue[args.Length];
					 for (int i = 0; i < jars.Length; ++i)
					 {
						 jars[i] = JsValue.FromObject(engine, args[i]);
					 }
					 thisLogger.log(() => "IV001B1 - starting invocation");
					 result = x.Invoke(x, jars).ToObject();
					 thisLogger.log(() => "IV001B2 - function result type = " + result != null ? result.GetType().FullName : "null");
					 return; 
				 }
				 catch (Exception ex)
				 {
					 thisLogger.log(() => "IV001C - Exception invoking function : ", ex);
					 ReportError(() => ex.Message, ex);
					 result = null;
					 return;
				 }
				 finally
				 {
					 lastExecutionLog.AppendLine(thisLogger.EndCapture());
				 }
			 }))
			{
				thisLogger.log(() => "IV001D - run with timeout failed. Returning null");
				return null;
			}
			return result;

		}

		public bool IsException()
		{
			return isException;
		}

		public bool IsReady()
		{
			return !paused;
		}
		bool paused = true;
		public void PauseOrPlay()
		{
			paused = !paused;
			GetLogger().log(() => GetHashCode() + ": 0216 : paused set to " + paused);
			if (!paused)
			{
				Activate(TimeSpan.FromMilliseconds(100));
			}
			else
			{
				Reset(scriptName);
			}
  
		}

		public void ReportError(Func<string> message, Exception ex)
		{
			GetLogger().log(() => "REPORT ERROR CALLED FROM STACKTRACE " + new StackTrace());
			GetLogger().log(() => message() + " : RE0259 - Error calling function", ex);
			paused = true;
			isException = true;
			engine = null;
			active = false;
			exceptionMessage = message();

		}
		string scriptName = "";
		private string scriptSource;
		private Engine engine;
		private string exceptionMessage;
		private bool isException = false;
		private StringBuilder lastExecutionLog = new();
		private Logger thisLogger;

		public string Script
		{
			get => scriptSource; set
			{
				if (scriptSource == null || !scriptSource.Equals(value))
				{
					scriptSource = value;
					scriptName = "internal" + GetHashCode();
				}
			}
		}

		public string LastExecutionLog { get => lastExecutionLog.ToString(); }

		public void UpdateScriptName(string sel)
		{
			GetLogger().log(()=>"UpdateScriptName invoked with " + sel);
			if (scriptName == null || !scriptName.Equals(sel))
			{
				GetLogger().log(() => "different status found. Resetting state for " + sel);
				Reset(sel);
			}

		}

		private void Reset(string sel)
		{
			active = false;
			GetLogger().log(() => "Reset with " + sel + " invoked");
			GetLogger().EndCapture();
			scriptName = sel;
			paused = true;
			engine = null;
			debugMode = true;
			exceptionMessage = "";
			isException = false;
			
		}

		public string GetScriptName()
		{
			return scriptName;
		}

		public void Load(string stateDefinition)
		{

			string[] values = stateDefinition.Split(new string[] { "///" }, StringSplitOptions.None);
			try
			{
				scriptName = values[0];
				debugMode = bool.Parse(values[1]);
				paused = bool.Parse(values[2]);
				if (!paused)
				{
					Activate(TimeSpan.FromMilliseconds(100));
				}
			}
			catch
			{


			}
			finally
			{
				GetLogger().log(() => GetHashCode() + ":0088-A Load invoked with definition " + stateDefinition);
				GetLogger().log(() => GetHashCode() + ":0088-B Our state is now " + Save());
				GetLogger().log(() => GetHashCode() + ":0088-C values read :" + string.Join("  ,  ", values));
			}
		}

		public string Save()
		{
			
			string result = scriptName + "///" + debugMode + "///" + paused;
			GetLogger().log(() => GetHashCode() + ":0097-A Save invoked. Persisted value = " + result);

			return result;
		}

		public string ExceptionMessage()
		{
			return exceptionMessage;
		}

		public bool HasCapturedState()
		{
			return GetLogger().IsCapturing();
		}

		public string PeekExecutionState()
		{
			return GetLogger().PeekCapture();
		}
	}

}