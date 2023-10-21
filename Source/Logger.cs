using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Verse;

namespace List_Everything
{
	public class Logger
	{
		private static readonly string sessionid = DateTime.Now.ToString("yyMMdd.HHmmss.ff");
		private static Logger _rootLogger;
		public static Logger RootLogger
		{
			get
			{
				if (_rootLogger == null)
				{
					_rootLogger = new Logger("RootLogger");
				}
				return _rootLogger;
			}
		}
		private static readonly Stopwatch watch = new();

		private StringBuilder capture;
		public void StartCapture()
		{
			capture = new();
			log(() => "full logs at " + filename);
		}

		private static object _logEnabled = null;
		private static Boolean onlyOnce = false;
		private static bool LogEnabled
		{
			get
			{
				if (_logEnabled == null)
				{
					string debugEnableFile = GetRootDirectory() + Path.DirectorySeparatorChar + "DebugMode";

					_logEnabled = File.Exists(debugEnableFile);
					if (!(bool)_logEnabled)
					{
						if (!onlyOnce)
						{
							onlyOnce = true;
							RootLogger.fatal(() => "Debug mode is turned off. Create a file named " + debugEnableFile + " to enable it, if you want to have detailed debug messages regarding this mod code");
						}

					}
				}

				return (bool)_logEnabled;
			}
		}


		private readonly string context;
		private StreamWriter writer;
		private string file;
		private string filename;



		public Logger(String context)
		{
			this.context = context;
		}
		public bool enabled()
		{
			return LogEnabled || (capture != null);
		}
		private StreamWriter getWriter()
		{
			if (writer == null)
			{
				filename = Regex.Replace("" + context, "\\W+", "");

				string directory;

				directory = GetRootDirectory();
				directory = directory + Path.DirectorySeparatorChar + sessionid;
				Directory.CreateDirectory(directory);
				filename = directory + Path.DirectorySeparatorChar + filename + "." + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log";

				Trace.WriteLine("Log file = " + filename);

				writer = File.CreateText(filename);
				this.file = filename;
			}
			return writer;
		}

		public string EndCapture()
		{
			if (capture != null)
			{
				string result = capture.ToString();
				capture = null;
				return result;
			}
			return "";

		}

		public static string GetRootDirectory()
		{
			string directory;
			try
			{
				directory = GenFilePaths.SaveDataFolderPath + Path.DirectorySeparatorChar + Mod.ModId;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Exception getting rimworld current directory. Using the current directory");
				Trace.WriteLine(ex.ToString());
				directory = System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Mod.ModId;
				Trace.WriteLine("Directory = " + directory);

			}

			return directory;
		}

		public string GetLogFile()
		{
			return file;
		}

		public void log(Func<object> v)
		{
			if (enabled())
			{
				FormatMessage(v);

				getWriter().Flush();
			}
		}

		private void FormatMessage(Func<object> v)
		{
			StreamWriter w = getWriter();
			StringBuilder b = new();
			b.Append("[" + Thread.CurrentThread.Name + "]");
			b.Append(" ");

			b.Append(v.Invoke());
			b.AppendLine();
			if(capture != null)
			{
				capture.Append(b);
			}
			b.Insert(0,DateTime.Now.ToString("HHmmss.fff"));
			b.Insert(0," ");
			b.Insert(0, Thread.CurrentThread.Name);
			w.Write(b);
			
		}

		public void VerseMessage(Func<object> v)
		{
			try
			{
				string content = "[" + Mod.Name + "]" + context + ":" + v.Invoke();
				if (capture != null)
				{
					capture.AppendLine(content);
				}
				Verse.Log.Message(content);
			}
			catch
			{

			}
		}

		public void error(Func<object> v, Exception e)
		{
			VerseMessage(() => v.Invoke() + "\n" + e.ToString());
			FormatException(v, e);
			getWriter().Flush();
		}

		public void Stacktrace()
		{
			StackTrace trace = new StackTrace();
			log(() => trace);
		}

		/**
		 * Creates a message, possibly sent to the Rimworld logs as well, but also in the root log.
		 */
		public void fatal(Func<object> v)
		{
			VerseMessage(v);
			FormatMessage(v);
			getWriter().Flush();
		}
		private void FormatException(Func<object> v, Exception e)
		{
			FormatMessage(v);
			Exception inner = e?.InnerException;
			StringBuilder traceString = new StringBuilder();
			string causedBy = "";
			Exception x = e;
			while(x != null)
			{
				
				StackTrace st = new StackTrace(x, true);
				StackFrame[] fr = st.GetFrames();
				traceString.Append(causedBy + x.GetType().FullName + ": " + x.Message);
				causedBy = "Caused by:";
				foreach (StackFrame f in fr)
				{
					traceString.AppendLine(" at " + f.GetMethod() + "(line " + f.GetFileLineNumber() + ",file " + f.GetFileName());
				}

				x = x.InnerException;
	  }


			
			if (capture != null)
			{
				capture.AppendLine(traceString.ToString());
			}
			getWriter().WriteLine(traceString);
			getWriter().Flush();
		}

		~Logger()
		{
			if (writer != null)
			{
				writer.Flush();
				writer.Dispose();
			}
		}

		public void log(Func<object> v, Exception ex)
		{

			if (enabled())
			{
				FormatException(v, ex);
			}
		}

	public bool IsCapturing()
	{
	  return capture != null;
	}

		public string PeekCapture()
		{
			if(capture != null)
			{
				return capture.ToString();
			}
			return "";
		}

  }


}
