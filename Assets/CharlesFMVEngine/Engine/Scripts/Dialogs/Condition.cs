using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CharlesEngine
{
	public enum FloatComparator
	{
		Equal,
		NotEqual,
		Greater,
		LessThen,
		GreaterOrEqual,
		LessThenOrEqual
	}

	[Serializable]
	public class Expression
	{
		public BaseVariable Variable;
		public bool Not;
		public FloatComparator Comparator;
		public int ComparisonValue;
		public bool CompareToAnotherVariable;
		public BaseVariable ComparisonVarValue;
		public string StringSetContainsValue;
		public override string ToString()
		{
			if (Variable == null) return (!Not).ToString();
			if (Variable is BoolVariable)
			{
				return (Not ? "!" : "") + Variable.name;
			}
			
			if( Variable is IntVariable && CompareToAnotherVariable)
			{
				return Variable.name + " " + GetCompString() + " " + ComparisonVarValue?.name;
			}

			if (Variable is StringSetVariable)
			{
				return (Not ? "!" : "") + Variable.name + ".Contains(" + StringSetContainsValue + ")";
			}

			if (CompareToAnotherVariable)
			{
				return  Variable.name + " " + GetCompString() + " " + ComparisonVarValue?.name;
			}
			return Variable.name + " " + GetCompString() + " " + ComparisonValue;
		}

		private string GetCompString()
		{
			switch (Comparator)
			{
				case FloatComparator.Equal: return "==";
				case FloatComparator.NotEqual: return "!=";
				case FloatComparator.Greater: return ">";
				case FloatComparator.LessThen: return "<";
				case FloatComparator.GreaterOrEqual: return ">=";
				case FloatComparator.LessThenOrEqual: return "<=";
			}
			return "";
		}

		public bool Eval()
		{
			if (Variable == null)
			{
				if (Application.isPlaying)
				{
					Debug.LogWarning("Variable was not set in condition");
				}
				return !Not;
			}
			if (Variable is BoolVariable)
			{
				BoolVariable var = (BoolVariable) Variable;
				return Not ? !var.RuntimeValue : var.RuntimeValue;
			}
			
			if (Variable is IEnumVariable)
			{
				IEnumVariable var = (IEnumVariable) Variable;
				switch (Comparator)
				{
					case FloatComparator.Equal: return var.GetValueAsInt() == ComparisonValue;
					case FloatComparator.NotEqual: return var.GetValueAsInt() != ComparisonValue;
					default: throw new Exception("Enum variable must use Equal or NotEqual operators");
				}
			}
		
			if( Variable is IntVariable )
			{
				IntVariable var = (IntVariable) Variable;
				int compareTo = ComparisonValue;
				if (CompareToAnotherVariable)
				{
					if (ComparisonVarValue == null)
					{
						Debug.LogWarning("Empty conditions value!");	
					}
					else
					{
						compareTo = ((IntVariable) ComparisonVarValue).RuntimeValue;
					}
				}
				switch (Comparator)
				{
					case FloatComparator.Equal: return var.RuntimeValue == compareTo;
					case FloatComparator.NotEqual: return var.RuntimeValue != compareTo;
					case FloatComparator.Greater: return var.RuntimeValue > compareTo;
					case FloatComparator.LessThen: return var.RuntimeValue < compareTo;
					case FloatComparator.GreaterOrEqual: return var.RuntimeValue >= compareTo;
					case FloatComparator.LessThenOrEqual: return var.RuntimeValue <= compareTo;
				}
			}

			if (Variable is StringSetVariable)
			{
				StringSetVariable svar = (StringSetVariable) Variable;
				var contains = svar.Contains(StringSetContainsValue);
				return Not ? !contains : contains;
			}
			
			// any other variable default to comparism with another variable of the same type if possible
			if (ComparisonVarValue == null)
			{
				Debug.LogWarning("Empty conditions value!");
				return false;
			}

			var variableAsComparable = Variable as IComparable;
			var compareAsComparable = ComparisonVarValue as IComparable;
			if (variableAsComparable == null || compareAsComparable == null)
			{
				Debug.LogWarning("Invalid type of variable in a condition:"+Variable.GetType().Name);
				return false;
			}

			var resultOfCompare = variableAsComparable.CompareTo(compareAsComparable);
			switch (Comparator)
			{
				case FloatComparator.Equal: return resultOfCompare == 0;
				case FloatComparator.NotEqual: return resultOfCompare != 0;
				case FloatComparator.Greater: return resultOfCompare > 0;
				case FloatComparator.LessThen: return resultOfCompare < 0;
				case FloatComparator.GreaterOrEqual: return resultOfCompare >= 0;
				case FloatComparator.LessThenOrEqual: return resultOfCompare <= 0;
			}
			return false;
		}

		public bool IsValid()
		{
			if (Variable == null) return false;
			if (Variable is BoolVariable)
			{
				return true;
			}
			if( CompareToAnotherVariable )
			{
				return ComparisonVarValue != null;
			}
			return true;
		}
	}

	[Serializable]
	public class AndGroup
	{
		public List<Expression> Expressions;
	}

	[Serializable]
	public class Condition
	{
		public List<AndGroup> AndGroups;
		public CustomCondition Custom;

		public bool IsEmpty => Custom == null && ( AndGroups == null || AndGroups.Count == 0 );

		public bool Eval()
		{
			if (Custom != null)
			{
				return Custom.Eval();
			}
		
			for (var i = 0; i < AndGroups.Count; i++)
			{
				var g = AndGroups[i];
				var groupValue = true;
				for (var j = 0; j < g.Expressions.Count; j++)
				{
					if (!g.Expressions[j].Eval())
					{
						groupValue = false;
						break;
					}
				}
				if (groupValue) return true;
			}
			return false;
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			for (var i = 0; AndGroups != null && i < AndGroups.Count; i++)
			{
				var g = AndGroups[i];
				if (g == null || g.Expressions == null) return "err";
				if( g.Expressions.Count > 1 ) str.Append("(");
				for (var j = 0; j < g.Expressions.Count; j++)
				{
					str.Append(g.Expressions[j]);
					if (j < g.Expressions.Count - 1)
					{
						str.Append(" && ");
					}
				}
				if( g.Expressions.Count > 1 ) str.Append(")");
				if (i < AndGroups.Count - 1)
				{
					str.Append(" || ");
				}
			}

			if (Custom != null)
			{
				str.Append("CUSTOM-"+Custom.GetType().Name);
			}
			if (IsEmpty)
			{
				str.Append("Empty");
			}
			return str.ToString();
		}

		public Condition GetCopy()
		{
			var result = new Condition();
			result.AndGroups = new List<AndGroup>();
			for (var i = 0; AndGroups != null && i < AndGroups.Count; i++)
			{
				var g = AndGroups[i];
				if (g == null || g.Expressions == null) continue;
				var ag = new AndGroup();
				ag.Expressions = new List<Expression>();
				foreach (var age in g.Expressions)
				{
					var newExp = new Expression
					{
						Comparator = age.Comparator,
						Not = age.Not,
						Variable = age.Variable,
						ComparisonValue = age.ComparisonValue
					};
					ag.Expressions.Add(newExp);
				}
				result.AndGroups.Add(ag);
			}

			return result;
		}

		public bool IsValid()
		{
			if (Custom != null)
			{
				return true;
			}

			if (AndGroups == null) return false;
			for (var i = 0; i < AndGroups.Count; i++)
			{
				var g = AndGroups[i];
				if (g.Expressions == null) return false;
				for (var j = 0; j < g.Expressions.Count; j++)
				{
					if (!g.Expressions[j].IsValid())
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}