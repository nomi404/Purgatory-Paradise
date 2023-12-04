using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace CharlesEngine
{
	public class VariableManager : ScriptableObject
	{
		public List<BaseVariable> Variables = new List<BaseVariable>();

		[SerializeField]
		private List<string> VariableNames = new List<string>();
		
		private List<BaseVariable> _profileVariables;
		private List<BaseVariable> _globalVariables;
		private List<BaseVariable> _memoryVariables;

		public List<BaseVariable> ProfileVariables => _profileVariables;
		public List<BaseVariable> GlobalVariables => _globalVariables;
		public List<BaseVariable> MemoryVariables => _memoryVariables;
	
		[Tooltip("Check this if you want to assign variables manually")]
		public bool Manual;
	
		[ContextMenu("Refresh")]
		public void RefreshList()
		{
			if ( !Manual )
			{
#if UNITY_EDITOR
				Variables = new List<BaseVariable>(FindAllVariablesInProject(VariableNames));
				var profileCheck = GetVariablesOfPersistenceType(PersistenceType.Profile);
				List<string> guids = new List<string>();
				foreach (var baseVariable in profileCheck)
				{
					guids.Add(baseVariable.Guid);
				}
				if (guids.Distinct().Count() != guids.Count)
				{
					Debug.LogError("duplicate guids detected", this);
				}
				Undo.RecordObject(this, "Refreshing var manager");
				EditorUtility.SetDirty(this);
				UnityEditor.AssetDatabase.SaveAssets();
#endif
			}
		
			_profileVariables = GetVariablesOfPersistenceType(PersistenceType.Profile);
			_globalVariables = GetVariablesOfPersistenceType(PersistenceType.Game);
			_memoryVariables = GetVariablesOfPersistenceType(PersistenceType.InMemory);
		}

		private static BaseVariable[] FindAllVariablesInProject(List<string> names)
		{
#if UNITY_EDITOR
			names.Clear();
			string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BaseVariable");  //FindAssets uses tags check documentation for more info
			BaseVariable[] a = new BaseVariable[guids.Length];
			for(int i =0;i<guids.Length;i++)         //probably could get optimized 
			{
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
				names.Add(Path.GetFileName(path).Replace(".asset",""));
				
				a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<BaseVariable>(path);
				if (a[i] == null)
				{
					Debug.LogError("variable not found " + path);
				}
				if (string.IsNullOrEmpty(a[i].Guid) || a[i].Guid != guids[i])
				{
					a[i].Guid = guids[i];
					EditorUtility.SetDirty(a[i]);
				}
			}
			UnityEditor.AssetDatabase.SaveAssets();
			return a;
#else
		return null;
#endif
		}

		public BaseVariable GetVariableById(string guid)
		{
			foreach (var variable in Variables)
			{
				if (variable.Guid == guid) return variable;
			}
			return null;
		}
		
		public BaseVariable GetVariableByName(string varname)
		{
			for (var i = 0; i < VariableNames.Count; i++)
			{
				if (VariableNames[i] != null && VariableNames[i].Equals(varname, StringComparison.InvariantCultureIgnoreCase)) //compare ignoring case
				{
					return Variables[i];
				}
			}
			return null;
		}

		public BaseVariable GetVariableByType<T>() where T : BaseVariable
		{
			foreach (var variable in Variables)
			{
				if (variable is T) return variable;
			}
			return null;
		}
	
		private List<BaseVariable> GetVariablesOfPersistenceType(PersistenceType t)
		{
			return Variables.Where(v => v != null && v.PersistanceType == t).ToList(); //generates lots of garbage
		}
	}
}
