using System;
using System.Collections.Generic;
using System.Reflection;

namespace List_Everything
{
	public class ReflectionUtils
	{
		private static Logger log = new Logger("ReflectionUtils");
		public static T fieldValue<T>(object o, string fieldName, T defaultValue)
		{
			log.log(() => "invoked fieldValue(" + o?.GetHashCode() + " , " + fieldName + " , " + defaultValue);
			if (o == null)
			{
				log.log(() => "null object. returning " + defaultValue);
				return defaultValue;
			}
			FieldInfo field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null)
			{
				log.log(() => o.GetHashCode() + " field found! " + field);
				T result = (T)field.GetValue(o);
				log.log(() => o.GetHashCode() + "field value: " + field);
				return result;
			}
			else
			{
				log.log(() => o.GetHashCode() + ";" + o.GetType() + "; field " + fieldName + " not found. Trying a property");
				PropertyInfo p = o.GetType().GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				if (p != null)
				{
					return (T)p.GetValue(o);
				}

				log.log(() => o.GetHashCode() + ";" + o.GetType() + "; property " + fieldName + " not found. Trying a property");
				return defaultValue;
			}

		}
		public static void SetValue<T>(object o, string fieldName, T value)
		{
			FieldInfo field = o.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public static void Inspect(Logger log, Object o)
		{

			if (o == null)
			{
				log.log(() => null);
				return;
			}
			log.log(() => o.GetType().FullName + (o.GetType().BaseType != null ? " :" + o.GetType().BaseType.FullName : ""));
			HashSet<Type> explored = new();
			Queue<Type> toExplore = new();
			toExplore.Enqueue(o.GetType());
			bool immediate = true;
			while (toExplore.Count > 0)
			{
				Type t = toExplore.Dequeue();
				if (immediate)
				{
					immediate = false;
					foreach (ConstructorInfo x in t.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
					{
						log.log(() => "  constructor " + x);
					}
				}
				else
				{
					log.log(() => "  FROM BASE TYPE " + t + ":");
				}
				foreach (FieldInfo x in t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
				{
					log.log(() => "  field " + x);
				}
				foreach (object x in t.GetMembers())
				{
					log.log(() => "  member " + x);
				}
				foreach (object x in t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
				{
					log.log(() => "  method " + x);
				}
				foreach (object x in t.GetInterfaces())
				{
					log.log(() => "  interfaces " + x);
				}
				explored.Add(t);
				if (t.BaseType != null && !explored.Contains(t.BaseType))
				{
					{
						toExplore.Enqueue(t.BaseType);
					}
				}
			}


		}


	}

}