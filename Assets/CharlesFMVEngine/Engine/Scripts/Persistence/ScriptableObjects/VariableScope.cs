using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{
	[CreateAssetMenu(fileName = "varScope",menuName ="Variables/Variable Scope")]
	public class VariableScope : BaseVariable {

		[Serializable]
		private class ScopeData : VariableData
		{
			public VariableRevision Revision;
		}
	
		[Tooltip("List of variables to include in this scope")]
		public List<BaseVariable> Variables = new List<BaseVariable>();
	
		public VariableRevision Revision { get; private set; } //not serialized
	
		public void Save()
		{
			Revision = new VariableRevision();
			Revision.LoadFromList(Variables);
#if UNITY_EDITOR	
			Debug.Log("Scope "+name+" saved");
#endif
		}

		public void Restore()
		{
			if (Revision == null || Revision.Empty())
			{
				Debug.LogWarning("No revision to revert to.");
				return;
			}
			var failCount = Revision.RestoreVariable(Variables);
#if UNITY_EDITOR		
			if (failCount > 0)
			{
				Debug.Log("Scope " + name + " failed to restore all variables (" + failCount+")");
			}
			else
			{
				Debug.Log("Scope " + name + " restored");
			}
#endif
		}

		public override void ResetValue()
		{
			foreach (var v in Variables)
			{
				v.ResetValue();
			}
		}

		public override VariableData GetData()
		{
			var d = new ScopeData
			{
				Revision = Revision
			};
			return d;
		}

		public override void LoadFromData(VariableData data)
		{
			Revision = ((ScopeData) data).Revision;
		}
	}
}
