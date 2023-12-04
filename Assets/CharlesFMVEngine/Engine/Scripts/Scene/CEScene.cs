using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
	public class CEScene: MonoBehaviour, IInputHandler
	{
		[Tooltip("This event is called before this scene is finished loading")]
		public UnityEvent OnScreenLoad = new UnityEvent();
		[Tooltip("This event is called after this scene loads")]
		public UnityEvent OnScreenShow = new UnityEvent();
		[Tooltip("This event is called before this scene exits")]
		public UnityEvent OnScreenHide = new UnityEvent();
	
		public bool FadeInOnStart = true;
		[Tooltip("Enable this in scenes where player should not be able to pause the game.")]
		public bool PauseMenuEnabled = true;
		[Tooltip("If checked this scene won't be saved with user profile. Use for menus, settings, etc.")]
		public bool UntrackedScene;
	
		[NonSerialized] public bool Loaded;
	
		private Camera _disabledCamera;
		private int _origCullingMask;
	
		private void Awake()
		{
			if (!Globals.Loaded)
			{
				_disabledCamera = Camera.main;
				if (_disabledCamera != null)
				{
					_origCullingMask = _disabledCamera.cullingMask;
					_disabledCamera.cullingMask = 0; //nothing is rendered
				}

				StartCoroutine(LoadGlobal());
			}
			
			try
			{
				OnScreenLoad.Invoke(); // this should fire before OnScreenShow
			}
			catch (Exception e)
			{
				Debug.LogError("There was an error while executing the OnScreenLoad event. "+e, gameObject);
			}
		}

		public IEnumerator LoadGlobal()
		{
			Debug.Log("Initializing Charles Engine, loading Globals...");
#if UNITY_EDITOR
			// Make sure Master scene is in Build settings, otherwise we cannot load it
			if (!EditorBuildSettings.scenes.Any(a => a.path.Contains("Master") ))
			{
				var guids = AssetDatabase.FindAssets("Master t:scene");
				foreach(var guid in guids )
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (path.Contains("Engine"))
					{
						var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
						editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(path,true));
						EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
						Debug.LogError("Master scene added to build settings, restart the game.");
						yield break;
					}
				}
				
			} 
#endif
			yield return SceneManager.LoadSceneAsync("Master",LoadSceneMode.Additive);
			yield return null;
			StartCoroutine( Globals.GameManager.LoadScene(gameObject.scene) );
		}

		public IEnumerator Load()
		{
			if (FindObjectOfType<VideoPool>() != null)
			{
				throw new Exception("VideoPool was not disposed properly");
			}

			var videoPrefab = Instantiate(Globals.VideosPrefab, transform.parent);
			Globals.Videos = videoPrefab.GetComponent<VideoPool>();
			
			var mainScript = FindObjectOfType<SceneMainScript>();
			if (mainScript == null)
			{
				FinishLoad();
				yield break;
			}
			mainScript.Prepare(); // Prepare the first video to play
			yield return null;
			FinishLoad();
		}
		
		private void FinishLoad()
		{
			Loaded = true;
			if (_disabledCamera)
			{
				_disabledCamera.cullingMask = _origCullingMask; //restore culling
			}
			if (FadeInOnStart)
			{
				Globals.Fade.Out(FadeManager.FadeType.SceneSwitch);
			}
			else
			{
				Globals.Fade.HideInstant();
			}

			try
			{
				OnScreenShow.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogError("There was an error while executing the OnScreenShow event. "+e, gameObject);
			}
		}

		public void Unload()
		{
			Loaded = false;
			OnScreenHide.Invoke();
		}
	
		public bool HandleInput()  
		{
			if (!Loaded || !Globals.Input || !PauseMenuEnabled) return false;
		
			if ( Globals.Input.GetKeyUp(InputAction.PauseGame) )
			{
				Globals.Pause.PauseGame();
				return true;
			}
			return false;
		}
	}
}
