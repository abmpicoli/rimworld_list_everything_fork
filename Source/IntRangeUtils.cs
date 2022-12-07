using System;
using Verse;

namespace List_Everything
{
  public class IntRangeUtils
  {
		public static bool Includes(IntRange countRange, int v)
		{
				return v >= countRange.min && v <= countRange.max;
		}
  }
}