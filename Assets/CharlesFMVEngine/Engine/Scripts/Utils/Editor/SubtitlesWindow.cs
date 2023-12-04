using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
#if CE_USE_I2Loc
using I2.Loc;
#endif

namespace CharlesEngine
{
	public class SubtitlesWindow : EditorWindow
	{
		private float _lastTime;
		private string _message;
		private string _stats;

		private bool _showImport;
		private bool _showExport;
		private TextAsset _selectedAsset;

		private string[] _tabOptions = {"Stats", "Import", "Export", "Validate"};
		private bool _importFoldout;
		private bool _importFromGDoc;
		private int _currentTab = 0;
		
		private const string ExportedFileStart = "SubtitleExport/SubtitleExport_";

		[MenuItem("Tools/Charles Engine/Subtitles Window", false, 201)]
		public static void ShowWindow()
		{
			GetWindow<SubtitlesWindow>(true, "Subtitles", true);
		}

		private void OnGUI()
		{
			_currentTab = GUILayout.Toolbar(_currentTab, _tabOptions);
			

			switch (_currentTab)
			{
				case 0:
					OnGUI_Stats();
					break;
				case 1:
					OnGUI_Import();
					break;
				case 2:
					OnGUI_Export();
					break;
				case 3:
					OnGUI_Validate();
					break;
			}

		}

		private void OnGUI_Stats()
		{
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
			{
				FillStats();
			}
			if (_stats == null)
			{
				FillStats();
			}
			
			EditorGUILayout.LabelField(_stats, EditorStyles.wordWrappedLabel);
		}

		//--------------------------------
		//===   IMPORT   ==============//
		//---
		
		private void OnGUI_Import()
		{
#if CE_USE_I2Loc
			if (LocalizationManager.Sources.Count == 0)
				LocalizationManager.UpdateSources();
#endif
			GUILayout.Space(20f);
			
			_importFoldout = EditorGUILayout.Foldout(_importFoldout, "Import from file");
			if (_importFoldout)
			{
				EditorGUILayout.HelpBox("Input txt file with subtitles", MessageType.Info);
				_selectedAsset =
					(TextAsset) EditorGUILayout.ObjectField("Import", _selectedAsset, typeof(TextAsset), false);

				if (_selectedAsset != null)
				{
					var fileExtension = _selectedAsset.name.Substring(_selectedAsset.name.Length - 2);
					LangEnum targetLang;
					var vali = Enum.TryParse(fileExtension, out targetLang);
					GUI.enabled = vali;
					string langString = "invalid";
					if (vali)
					{
#if CE_USE_I2Loc
						var langs = LocalizationManager.Sources[0].mLanguages;
						foreach (var languageData in langs)
						{
							if (languageData.Code == fileExtension)
							{
								langString = languageData.Name;
							}
						}
#else
						langString = targetLang.ToString();
#endif
					}

					if (GUILayout.Button("Import for language: " + langString))
					{
						ImportFromFile(_selectedAsset, targetLang);
					}

					GUI.enabled = true;
				}
			}
		}

		private void ImportFromFile(TextAsset asset, LangEnum targetLang)
		{
			var allText = asset.text;
			if (allText.IndexOf("@@@@@", StringComparison.InvariantCulture) < 0)
			{
				Debug.LogError("This is not a valid file to import.");
				return;
			}

			string[] results = Regex.Split(allText, "@@@@@");

			var langString = Enum.GetName(typeof(LangEnum), targetLang);
			int successCount = 0;
			int skippedCount = 0;
			foreach (var str in results)
			{
				if (string.IsNullOrWhiteSpace(str))
				{
					skippedCount++;
					continue;
				}

				var firstNewLineIndex = str.IndexOf(Environment.NewLine, StringComparison.InvariantCulture);
				var firstSeparatorIndex = str.IndexOf("|", StringComparison.InvariantCulture);
				string guid = str.Substring(0, firstSeparatorIndex);
				var subtitlesText = str.Substring(firstNewLineIndex + Environment.NewLine.Length).Trim();
				if (ImportItem(guid,subtitlesText,langString))
				{
					successCount++;
				}
			}
			AssetDatabase.Refresh();
			Debug.Log("Import success:" + successCount + " failed:" + (results.Length - successCount - skippedCount));
		}

		private bool ImportItem(string guid, string text, string lang)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrWhiteSpace(path))
			{
				Debug.LogError("Import error on part (guid broken or asset deleted) " + guid + " ("+text.Substring(0,60));
				return false;
			}

			if (string.IsNullOrWhiteSpace(text))
			{
				Debug.LogError("Import error on part (text is empty) " + guid);
				return false;
			}
			
			if (!path.EndsWith(lang + ".txt"))
			{
				string dirPath;
				path = FindNewPathForLang(path, lang, out dirPath);
				if (!File.Exists(path))
				{
					Debug.Log("No subs in " + lang + " found, creating new files.");
					path = path.Substring(0, path.Length - 3) + "srt"; //change to srt so it gets caught by the asset importer
					Directory.CreateDirectory(dirPath);
				}
			}

			try
			{
				File.WriteAllText(path, text);
			}
			catch (Exception e)
			{
				Debug.LogError("SubtitleError:" + path + " \n\n" + e);
				return false;
			}

			return true;
		}

		
		
		//--------------------------------
		//===   EXPORT   ==============//
		//---

		private void OnGUI_Export()
		{
			GUILayout.Space(20f);

			EditorGUILayout.HelpBox("Exports txt files with all subtitles by language to SubtitleExport folder",
				MessageType.Info);
			if (GUILayout.Button("Export All Subtitles"))
			{
				ExportAllSubtitles();
			}

			GUILayout.Space(20f);
			if (GUILayout.Button("Build Asset Bundles"))
			{
				BuildAssetBundle(BuildTarget.StandaloneWindows);
			}


			if (Time.realtimeSinceStartup - _lastTime < 5)
			{
				EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedMiniLabel);
			}
			else
			{
				_message = null;
			}
		}

		private void ExportAllSubtitles()
		{
			var allSubtitles = AssetDatabase.FindAssets("l:Subtitles");
			Array.Sort(allSubtitles, (a, b) =>
				String.Compare(Path.GetFileName(AssetDatabase.GUIDToAssetPath(a)), Path.GetFileName(AssetDatabase.GUIDToAssetPath(b)), StringComparison.InvariantCulture)
			);

			foreach (LangEnum lang in GetLangs())
			{
				var langString = Enum.GetName(typeof(LangEnum), lang);

				string exportedFileName = ExportedFileStart + langString + ".txt";
				var langStringTxt = langString + ".txt";

				var contents = new StringBuilder();
				foreach (var guid in allSubtitles)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (path.EndsWith(langStringTxt))
					{
						contents.AppendLine(Environment.NewLine);
						contents.Append("@@@@@" + guid + "|" + BaseFileName(Path.GetFileName(path)));
						contents.AppendLine(Environment.NewLine);
						contents.Append(File.ReadAllText(path));
					}
				}
				FileInfo fileInfo = new FileInfo(exportedFileName);
				if (!fileInfo.Exists && fileInfo.Directory != null)
					Directory.CreateDirectory(fileInfo.Directory.FullName);
				File.WriteAllText(exportedFileName, contents.ToString().Trim(), Encoding.UTF8);
				Log("Subtitles " + lang + " exported " + exportedFileName);
			}
		}

		public static void BuildAssetBundle(BuildTarget target)
		{
			string folderName = "AssetBundles";
			string filePath = Application.streamingAssetsPath + "/" + folderName;
			Directory.CreateDirectory(filePath);
			var allSubtitles = AssetDatabase.FindAssets("l:Subtitles");
			if (allSubtitles == null || allSubtitles.Length == 0) return;
			
			AssetBundleBuild[] buildMap = new AssetBundleBuild[GetLangs().Count()];
			var i = 0;
			foreach (LangEnum lang in GetLangs())
			{
				var langString = Enum.GetName(typeof(LangEnum), lang);
				var langStringTxt = langString + ".txt";

				buildMap[i].assetBundleName = "subtitles_"+langString;

				var subsAssets = new List<string>();
				var subsAssetsNicknames = new List<string>();
				foreach (var guid in allSubtitles)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (path.EndsWith(langStringTxt))
					{
						subsAssets.Add(path);
						var filename = Path.GetFileName(path);
						subsAssetsNicknames.Add(filename.Substring(0,filename.Length-7)); // remove file extension and language suffix
					} 
				}
				buildMap[i].assetNames = subsAssets.ToArray();	
				buildMap[i].addressableNames = subsAssetsNicknames.ToArray();	
				i++;
			}

			BuildPipeline.BuildAssetBundles(filePath, buildMap, BuildAssetBundleOptions.None, target);
		}
		
		private void OnGUI_Validate()
		{
			Dialogues currentDialog = null;

			if (Selection.activeTransform)
			{
				currentDialog = Selection.activeTransform.GetComponent<Dialogues>();
			}
			
			if (currentDialog == null)
			{
				EditorGUILayout.HelpBox("No Dialogue selected", MessageType.Warning);
				if (GUILayout.Button("Validate all dialog scenes"))
				{
					ValidateAllScenes();	
					Log("Validation complete");
				}
			}
			else
			{
				EditorGUILayout.LabelField(Selection.activeTransform.name, EditorStyles.boldLabel);

				if (GUILayout.Button("Validate dialogue"))
				{
					ValidateDialogue(currentDialog, EditorSceneManager.GetActiveScene().name);
					Log("Validation complete");
				}
			}

			if (Time.realtimeSinceStartup - _lastTime < 15)
			{
				EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedMiniLabel);
			}
			else
			{
				_message = null;
			}
		}

		private void ValidateAllScenes()
		{
			EditorUtility.DisplayProgressBar("Validating dialog scenes", "", 0.1f);
			int i = 0;
			foreach (Scene scene in GetSavedScenes())
			{
				GameObject[] rootObjects = scene.GetRootGameObjects();
				foreach (GameObject rootObject in rootObjects)
				{
					var dialogs = rootObject.GetComponentInChildren<Dialogues>(); // Find dialogs to validate
					if( dialogs != null )
						ValidateDialogue(dialogs, scene.name);
				}

				i++;
				EditorUtility.DisplayProgressBar("Validating dialog scenes", "", 0.1f+i/60f);
			}
			EditorUtility.ClearProgressBar();
			Log("Validation complete");
		}
		
		private void ValidateDialogue(Dialogues dialog, string name)
		{
			var clips = new HashSet<VideoClip>(); // to remove duplicates
			foreach (var tree in dialog.Trees)
			{
				clips.UnionWith( tree.GetAllVideoClips() );
			}
			
			int cnt = clips.Count;
			if (cnt == 0) return;
			
			int err = 0;
			foreach (var c in clips)
			{
				foreach (LangEnum lang in GetLangs())
				{
					var langString = Enum.GetName(typeof(LangEnum), lang);
					var result = GetSubs(c.name, langString);
					if (result == null)
					{
						err++;
						Debug.LogWarning("Missing " + langString?.ToUpper() + " subtitles for " + Path.GetFileName(c.originalPath));
					}else if (result.EndSeconds() > c.frameCount / c.frameRate)
					{
						err++;
						Debug.LogWarning("Subtitles longer than video: " + Path.GetFileName(c.originalPath));
					}
				}
			}
			Log(name+" subtitles checked:" + cnt + ", errors found:" + err);
		}

		private SubtitlesData GetSubs(string clipName, string langString)
		{
			var allFound = AssetDatabase.FindAssets(clipName + "_" + langString);
			if (allFound.Length == 1)
			{
				var fileGuid = allFound[0];
				var path = AssetDatabase.GUIDToAssetPath(fileGuid);
				var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				return asset == null ? null : new SubtitlesData(asset.text, path);
			}
			if (allFound.Length > 1)
			{
				Log("Multiple subtitles for:" + clipName + "_" + langString);
			}
			return null;
		}

		/**
	 * Takes path like:  Assets/Vid/Resources/subs/cs/video_cs.txt and lang string en
	 * returns: Assets/Vid/Resources/subs/en/video_en.txt
	 */
		private string FindNewPathForLang(string path, string langString, out string dirPath)
		{
			var filename = Path.GetFileName(path);
			var filePath = path.Substring(0, path.Length - filename.Length);
			var lastUnderscore = filename.LastIndexOf("_", StringComparison.Ordinal);
			filename = filename.Substring(0, lastUnderscore + 1) + langString + ".txt";
			if (filePath[filePath.Length - 4] == '/' && filePath[filePath.Length - 1] == '/')
			{
				filePath = filePath.Substring(0, filePath.Length - 3) + langString + "/";
			}
			dirPath = filePath;
			return filePath + filename;
		}

		private void FillStats()
		{
			var sb = new StringBuilder();
			var allSubtitles = AssetDatabase.FindAssets("l:Subtitles");
			int total = 0;
			foreach (LangEnum lang in GetLangs())
			{
				var langString = Enum.GetName(typeof(LangEnum), lang);
				sb.Append(langString + ":");
				int cnt = 0;
				var langStringTxt = langString + ".txt";
				foreach (var guid in allSubtitles)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (path.EndsWith(langStringTxt))
					{
						cnt++;
					}
				}
				total += cnt;
				sb.Append(cnt + " \n");
			}
			sb.Append("other:" + (allSubtitles.Length - total));
			_stats = sb.ToString();
		}

		private string BaseFileName(string fileName)
		{
			// removes the _cs.txt suffix
			return fileName.Substring(0, fileName.Length - 7).ToLowerInvariant();
		}

		private void Log(string s)
		{
			_message += s + "\n";
			_lastTime = Time.realtimeSinceStartup;
			Debug.Log(s);
		}

		private static IEnumerable<LangEnum> GetLangs()
		{
			return Enum.GetValues(typeof(LangEnum)).Cast<LangEnum>();
		}
		
		private static IEnumerable<Scene> GetSavedScenes() {
			string[] guids = AssetDatabase.FindAssets("t:Scene");
			foreach (string guid in guids) {
				yield return EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
			}
		}

	}	
}