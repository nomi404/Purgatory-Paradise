using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	public class InitNewSceneMenu : MonoBehaviour {

		[MenuItem("Tools/Charles Engine/Init New Scene/Generic",false, 50)]
		private static void InitSceneGeneric()
		{
			var currentCamera = FindObjectOfType<Camera>();
			if (currentCamera != null)
			{
				DestroyImmediate(currentCamera.gameObject);
			}

			GameObject cameraPrefab = null;
			var guids = AssetDatabase.FindAssets("SceneCamera t:prefab");
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.Contains("Engine"))
				{
					cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					break;
				}
			}

			if (cameraPrefab != null)
			{
				var camera = (GameObject) PrefabUtility.InstantiatePrefab(cameraPrefab);
				var settings = Resources.Load<CEngineSettings>("CEngineSettings");
				if (settings != null)
				{
					camera.GetComponent<Camera>().orthographicSize = settings.Resolution.y / 2;
				}
			}
			else
			{
				Debug.LogError("Cannot find camera prefab");
			}

			if (GameObject.Find("SceneManager") == null)
			{
				var sceneManagerObj = new GameObject("SceneManager");
				sceneManagerObj.AddComponent<CEScene>();
			}

			if (GameObject.Find("SceneRoot") == null)
			{
				var sceneRootObj = new GameObject("SceneRoot");
				sceneRootObj.AddComponent<CESceneRoot>();
				sceneRootObj.AddComponent<ScreenScaler>();
		
				var background = new GameObject("Background");          
				background.transform.SetParent(sceneRootObj.transform); 
				background.transform.position = Vector3.forward;    //so it stays behind everything       
				background.transform.localScale = Vector3.one;
				background.AddComponent<SpriteRenderer>();
			}

			var scriptsobject = new GameObject("Scripts");


			string scenePath = scriptsobject.scene.path;
		

			// Set the Build Settings window Scene list
			var scenesCurrent = EditorBuildSettings.scenes;
			var isAlreadyInBuild = false;
			foreach (var sc in scenesCurrent)
			{
				if (sc.path == scenePath)
				{
					isAlreadyInBuild = true;
					break;
				}
			}

			if (!isAlreadyInBuild)
			{
				var currentSceneSettings = new EditorBuildSettingsScene(scenePath, true);
				var newList = new List<EditorBuildSettingsScene>(scenesCurrent);
				newList.Add(currentSceneSettings);
				EditorBuildSettings.scenes = newList.ToArray();
			}
		}
		
		[MenuItem("Tools/Charles Engine/Init New Scene/Dialog",false, 50)]
		private static void InitSceneDialog()
		{
			InitSceneGeneric();
			if (GameObject.Find("Dialogues") == null)
			{
				var diaObj = new GameObject("Dialogues");
				diaObj.transform.SetSiblingIndex(2);
				var dii = diaObj.AddComponent<Dialogues>();

		
				var diaManagerObj = new GameObject("DialogManager");    
				diaManagerObj.transform.SetSiblingIndex(2);
				var dm = diaManagerObj.AddComponent<DialogManager>();
				dm.Dialogues = dii;

				var sm = GameObject.Find("SceneManager").GetComponent<CEScene>();
				UnityEditor.Events.UnityEventTools.AddPersistentListener(sm.OnScreenShow, dm.StartDialog);
			}

			if (GameObject.Find("Background") != null )
			{
				DestroyImmediate(GameObject.Find("Background"));
			}
		}
	}
}
