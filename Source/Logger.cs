using System;
using System.Diagnostics;
using System.IO;
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

		public Logger(String context)
		{
			this.context = context;
		}
		public Func<bool> enabled = () => LogEnabled;

		private StreamWriter getWriter()
		{
			if (writer == null)
			{
				string filename = Regex.Replace("" + context, "\\W+", "");

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

		private static string GetRootDirectory()
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
			w.Write("[" + Thread.CurrentThread.Name + "]");
			w.Write(" ");

			w.Write(DateTime.Now.ToString("HHmmss.fff"));
			w.Write(" ");
			w.Write(v.Invoke());
			w.WriteLine();

		}

		public void VerseMessage(Func<object> v)
		{
			try
			{
				Verse.Log.Message("["+Mod.Name+"]" + context + ":" + v.Invoke());
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
			getWriter().WriteLine(Regex.Replace("" + e?.ToString(), "(?m)^", "\n  "));
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
	}


}
