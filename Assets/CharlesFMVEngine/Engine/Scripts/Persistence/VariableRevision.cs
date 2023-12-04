using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{
	[Serializable]
	public class VariableRevision {
	
		[SerializeField] Dictionary<string, VariableData> Data = new Dictionary<string, VariableData>();
	
		public void LoadFromList(List<BaseVariable> list)
		{
			Data.Clear();
			foreach (var v in list)
			{
				if (Data.ContainsKey(v.Guid)) continue;
				Data.Add(v.Guid, v.GetData().Clone()); //clone data to ignore if they change in the future
			}
		}

		public int RestoreVariable(List<BaseVariable> list)
		{
			var failCount = 0;
			foreach (var v in list)
			{
				VariableData d;
				if (Data.TryGetValue(v.Guid, out d))
				{
					v.LoadFromData(d);
				}
				else
				{
					failCount++;
				}
			}
			return failCount;
		}
	
		public bool Empty()
		{
			return Data.Count == 0;
		}

		public Dictionary<string, VariableData> GetData()
		{
			return Data;
		}
	}
}
