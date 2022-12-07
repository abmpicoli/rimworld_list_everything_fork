using System;
using System.Collections.Generic;
using UnityEngine;

namespace List_Everything
{
	public class CompareType : ICompareType
	{
		static CompareType()
		{
			Add(new CompareTypeFactory("greater_than",(data,criteria) => data.CurrentState.Count > (int)criteria, TexButton.GreaterThan),">");
			Add(new CompareTypeFactory("less_than",(data,criteria) => data.CurrentState.Count < (int)criteria, TexButton.LessThan),"<");
			Add(new CompareTypeFactory("equals",(data,criteria) => data.CurrentState.Count == (int)criteria, TexButton.Equals),"=");
		}
		private readonly Func<FindAlertData, bool> function;
		private readonly Texture2D symbol;
	private readonly object criteria;
	private readonly string caption;
	private static readonly ICompareType EMPTY = new CompareType((s) => false, TexButton.Unknown,null);

		internal CompareType(ICompareTypeFactory factory,Func<FindAlertData, bool> function, Texture2D symbol, object criteria,string caption)
		{
			this.function = function;
			this.symbol = symbol;
			this.criteria = criteria;
			this.caption = caption;
			this.factory = factory;
		}
		private static Dictionary<string, ICompareTypeFactory> knownTypes = new();
		public static ICompareType Get(string name,FindAlertData data,object criteria)
		{
			ICompareTypeFactory result;
			if(knownTypes.TryGetValue(name,out result))
			{
				return result.NewInstance(data, criteria);
			}
			return EMPTY;
		}

	public bool Evaluate()
	{
			
	}

	public Texture2D Icon()
	{
		return symbol;
	}

	
	public static CompareType NextComparator(ICompareType countComp)
		{
			Dictionary<string, ICompareType>.ValueCollection x = knownTypes.Values;
	}
  }

}