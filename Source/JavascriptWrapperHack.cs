using System.Collections.Generic;

namespace List_Everything
{
	public class JavascriptWrapperHack
	{
		private object o;
		private Logger log;

		public JavascriptWrapperHack(object o, Logger thisLog)
		{
			this.o = o;
			this.log = thisLog;
		}

		/**
		* Gets a field property by reflection
		*/
		public object get(string key, object defaultValue = null)
		{
			return ReflectionUtils.fieldValue(o, key, defaultValue);
		}
	}


}