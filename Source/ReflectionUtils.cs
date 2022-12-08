using System;
using System.Reflection;

namespace List_Everything
{
	public class ReflectionUtils
	{
		private static Logger log = new Logger("ReflectionUtils");
		public static T fieldValue<T>(object o, string fieldName, T defaultValue)
		{
			log.log(()=>"invoked fieldValue(" + o?.GetHashCode() + " , " + fieldName + " , " + defaultValue);
			if (o == null)
			{
				log.log(()=>"null object. returning " + defaultValue);
				return defaultValue;
			}
			FieldInfo field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				log.log(()=>o.GetHashCode() + " field found! " + field);
				T result = (T)field.GetValue(o);
				log.log(()=>o.GetHashCode() + "field value: " + field);
				return result;
	  }
			else
			{
				log.log(()=>o.GetHashCode() + ";" + o.GetType() + "; field " + fieldName + " not found. Trying a property");
				PropertyInfo p = o.GetType().GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				if(p != null)
				{
					return (T)p.GetValue(o);
				}
				log.log(()=>o.GetHashCode() + ";" + o.GetType() + "; property " + fieldName + " not found. Trying a property");
				return defaultValue;
			}

		}
		public static void SetValue<T>(object o, string fieldName, T value)
		{
			FieldInfo field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}
	}

}