using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharlesEngine
{
	[Serializable]
	public class StringSet : IComparable
	{
		public List<string> List;

		public int CompareTo(object obj)
		{
			var other = obj as StringSet;

			if (other?.List.Count != List.Count)
			{
				return -1;
			}
		
			for (int i = 0; i < other.List.Count; i++)
			{
				if (other.List[i] != List[i]) return 1;
			}
		
			return 0;
		}

		public StringSet()
		{
			List = new List<string>();
		}

		public StringSet(StringSet reference)
		{
			List = new List<string>(reference.List);
		}
	}

	[CreateAssetMenu(fileName = "stringsVar",menuName ="Variables/String Set")]
	public class StringSetVariable : Variable<StringSet>
	{
		public event Action<string> OnItemAdded;
	
		public void AddItem(string item)
		{
			if (RuntimeValue.List.Contains(item)) return;
			RuntimeValue.List.Add(item);
			OnItemAdded?.Invoke(item);
		}

		public void RemoveItem(string item)
		{
			if (RuntimeValue.List.Contains(item))
			{
				RuntimeValue.List.Remove(item);
			}
		}

		public bool Contains(string item)
		{
			return _runtimeVal.List.Contains(item);
		}
		
		public override void ResetValue()
		{
			RuntimeValue = new StringSet(DefaultValue);
		}
		
		public void ResetDefault()
		{
			DefaultValue = new StringSet();
			ResetValue();
		}

		public int Count()
		{
			return RuntimeValue.List.Count;
		}

		[ContextMenu("Print")]
		public void PrintToConsole()
		{
			var ar = RuntimeValue.List.ToArray();
			Array.Sort(ar, (s,b) => s.CompareTo(b));
			Debug.Log( string.Join(",",ar) );
		}
	}
}