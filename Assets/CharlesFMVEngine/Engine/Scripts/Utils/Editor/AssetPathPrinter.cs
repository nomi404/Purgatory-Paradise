

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
	public class AssetLocPrinter : MonoBehaviour
	{
		
		/**
		 * http://wiki.unity3d.com/index.php/AssetPathPrinter
		*/
		
		[MenuItem("Assets/Selection/Print Asset Location %j")]
		private static void CopyAssetLocations()
		{
			string path = "";
			for (int i = 0; i < Selection.objects.Length; i++)
			{
				var o = Selection.objects[i];
				path += (i > 0 ? "\n" : "") + AssetDatabase.GetAssetPath(o);
			}
			Debug.Log(path);
			EditorGUIUtility.systemCopyBuffer = path; //adds text to clipboard
		}
		
		[MenuItem("Assets/Find References in Project",false, 28)]
		private static void FindReferencesToAsset(MenuCommand data)
		{
			var selected = Selection.activeObject;
			if (selected)
			{
				EditorUtility.DisplayProgressBar("Finding references in scenes", "", 0f);

				var searchedObjectPath = AssetDatabase.GetAssetPath(selected.GetInstanceID());
				Debug.Log("Asset ("+searchedObjectPath+") is referenced in:");
				var allScenes = AssetDatabase.FindAssets("t:scene");
				var allScenesPaths = allScenes.Select(AssetDatabase.GUIDToAssetPath).ToArray();
				int found = 0;
				for (var i = 0; i < allScenesPaths.Length; i++)
				{
					var scenePath = allScenesPaths[i];
					if( scenePath.Contains("Master.unity") ) continue; // skip master scene
					var dep = AssetDatabase.GetDependencies(scenePath, false); // false is important here, otherwise it includes all assets reference in all scenes accessible from this one
					if(dep.Contains(searchedObjectPath))
					{
						Debug.Log(scenePath);
						found++;
					}
					EditorUtility.DisplayProgressBar("Finding references in scenes", "Searching", (float) i/allScenesPaths.Length );
				}
				Debug.Log("-- Found "+found+" scenes referencing "+Path.GetFileName(searchedObjectPath)+" --");
				EditorUtility.ClearProgressBar();
			}
		}
		
		[MenuItem("Tools/Charles Engine/Tools/Fix Scene References", false, 353)]
		private static void FixSceneRefs()
		{
			EditorUtility.DisplayProgressBar("Fixing scene references", "", 0f);

			var scenes = GetSavedScenes();
			int i = 0;
			foreach (Scene scene in scenes)
			{
				GameObject[] rootObjects = scene.GetRootGameObjects();
				foreach (GameObject rootObject in rootObjects)
				{
					var sceneRefs = rootObject.GetComponentsInChildren<SwitchToScene>();
					var change = false;
					foreach (var swtc in sceneRefs)
					{
						if(swtc.Destination.Refresh()) change = true;
					}
					if (change)
					{
						EditorSceneManager.SaveScene(scene);
					}
				}
				i++;
				EditorUtility.DisplayProgressBar("Fixing scene references", "", (float) i/26 );
			}
			EditorUtility.ClearProgressBar();
		}
		
		private static IEnumerable<Scene> GetSavedScenes() {
			string[] guids = AssetDatabase.FindAssets("t:Scene");
			foreach (string guid in guids) {
				yield return EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
			}
		}
	}
}