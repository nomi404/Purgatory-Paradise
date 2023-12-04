using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
	public class VariablePersistence : MonoBehaviour
	{
		public VariableManager VariableManager;

		private readonly BinaryFormatter _formatter = new BinaryFormatter();

		private static string SavePath = "Temp"; //overriden in a build

		private string ProfileName = null;
		
		private Dictionary<string, float> _playTimes = new Dictionary<string, float>();
		private Dictionary<string, long> _loadTimes = new Dictionary<string, long>();
		
		private void Start()
		{
			if (VariableManager == null)
			{

#if UNITY_EDITOR    // find one in assets
				var managers = AssetDatabase.FindAssets("VariableManager");
				foreach (var guid in managers)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var manager = AssetDatabase.LoadAssetAtPath<VariableManager>(path);
					if (manager)
					{
						VariableManager = manager;
						break;
					}
				}
				// no variable manager in project, let's create one
				if (VariableManager == null)
				{
					var asset = ScriptableObject.CreateInstance<VariableManager>();
					AssetDatabase.CreateAsset (asset, "Assets/VariableManager.asset");
					AssetDatabase.SaveAssets ();
					AssetDatabase.Refresh();
					VariableManager = asset;
					Debug.LogWarning("Creating a new variable manager",asset);
				}
#else
				Debug.LogError("No variable manager assigned!");
				return;
#endif
			}
			VariableManager.RefreshList();

//#if UNITY_EDITOR
			SavePath = Application.persistentDataPath;
//#endif
		}

		public void SetProfile(string profileName)
		{
			ProfileName = profileName;
		}

		public void SaveProfile(string currentScene)
		{
			if (string.IsNullOrEmpty(ProfileName)) return;
			var saveFile = new SaveFile();
			
			saveFile.LastScene = currentScene;

			var profileVars = new VariableRevision();
			profileVars.LoadFromList(VariableManager.ProfileVariables);
			saveFile.ProfileVariables = profileVars;
			// calc time
			if (!_loadTimes.ContainsKey(ProfileName)) // fresh profile
			{
				_loadTimes[ProfileName] = DateTime.Now.Ticks;
				_playTimes[ProfileName] = 0;
			}
			float elapsedTime = (float) TimeSpan.FromTicks(DateTime.Now.Ticks - _loadTimes[ProfileName]).TotalSeconds;
			saveFile.PlayedSeconds = _playTimes[ProfileName] + elapsedTime;
			_loadTimes[ProfileName] = DateTime.Now.Ticks;
			_playTimes[ProfileName] = saveFile.PlayedSeconds;
			
			FileStream file = File.Create(GetSaveFileName(ProfileName));
			_formatter.Serialize(file, saveFile);
			file.Close();
			Log("Save Profile success {"+currentScene+"}:" + GetSaveFileName(ProfileName), PERSISTANCE);
		}

		/**
		 * Not sure if we need globals at all...
		 */
		private void SaveGlobals()
		{
			var globalRevision = new VariableRevision();
			globalRevision.LoadFromList(VariableManager.GlobalVariables);
			FileStream file = File.Create(GetSaveFileName("_global"));
			_formatter.Serialize(file, globalRevision);
			file.Close();
		}

		public SaveFile LoadProfile(string profileId)
		{
			SetProfile(profileId);
			var filename = GetSaveFileName(profileId);
			if (File.Exists(filename))
			{
				SaveFile data;
				try
				{
					FileStream file = File.Open(filename, FileMode.Open);
					data = (SaveFile) _formatter.Deserialize(file);
					data.ProfileVariables.RestoreVariable(VariableManager.ProfileVariables);
					file.Close();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return null;
				}

				LoadGlobals();
				_loadTimes[profileId] = DateTime.Now.Ticks;
				_playTimes[profileId] = data.PlayedSeconds;
				return data;
			}
			
			Log("Profile save not found (" + filename + ")", PERSISTANCE);			
			return null;
		}

		private void LoadGlobals()
		{
			var filename = GetSaveFileName("_global");
			if (File.Exists(filename))
			{
				FileStream file = File.Open(filename, FileMode.Open);
				var data = (VariableRevision) _formatter.Deserialize(file);
				var failCount = data.RestoreVariable(VariableManager.GlobalVariables);
				if (failCount > 0)
				{
					Debug.LogWarning("Load failed in " + failCount + " cases.");
				}
				file.Close();
			}
		}

		private string GetSaveFileName(string profileId)
		{
			return Path.Combine(SavePath, profileId + ".savefile");
		}

		public void ResetMemoryVars()
		{
			Log("Resetting memory-only variables", PERSISTANCE);
			foreach (var varr in VariableManager.MemoryVariables)
			{
				if (varr == null) continue;
				varr.ResetValue();
			}
		}
		
		public void ResetProfileVars()
		{
			Log("Resetting profile variables", PERSISTANCE);
			foreach (var varr in VariableManager.ProfileVariables)
			{
				if (varr == null) continue;
				varr.ResetValue();
			}
		}

		public void ResetAll()
		{
			ResetMemoryVars();
			ResetProfileVars();
		}

		public bool HasSavedGame(string profileName)
		{
			var filename = GetSaveFileName(profileName);
			if (File.Exists(filename))
			{
				return true;
			}
			Log("Profile save not found (" + filename + ")", PERSISTANCE);
			return false;
		}

		public void DeleteProfile(string profileName)
		{
			var filename = GetSaveFileName(profileName);
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}

			_loadTimes.Remove(profileName);
			_playTimes.Remove(profileName);
		}
	}
}