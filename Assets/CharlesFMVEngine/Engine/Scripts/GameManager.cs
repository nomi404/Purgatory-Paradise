using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
	public class GameManager : MonoBehaviour
	{
		public GameObject TransitionCamera;
		[HideInInspector]
		public UnityEvent OnSceneLoad;
		
		public bool IsLoading;

		private AsyncOperation _preloadingOperation;
		private string _preloadedScene;
		
		public void LoadScene(string sceneName, bool fadeOut = false)
		{
			Debug.Log("LoadScene:" + sceneName);
			if (string.IsNullOrEmpty(sceneName))
			{
				Debug.LogWarning("Scene name must be specified:" + sceneName);
				return;
			}
			
			if (SceneManager.GetActiveScene().name == sceneName)
			{
				Debug.LogWarning("Scene already loaded:" + sceneName);
				return;
			}

			if (IsLoading)
			{
				throw new Exception("LoadScene called during another load scene. Are you trying to load 2 scenes at once? "+sceneName);
			}
			StartCoroutine(LoadSceneRoutine(sceneName, fadeOut));
		}

		private IEnumerator LoadSceneRoutine(string sceneName, bool fadeOut = false)
		{
			_inputEnabled = false;
			IsLoading = true;
			// Cleanup
			if (Globals.Sounds != null) // can happen at startup
			{
				Globals.Sounds.StopAll();
				Globals.Choices.Hide(false);
			}

			if (fadeOut)
			{
				yield return Globals.Fade.In(FadeManager.FadeType.SceneSwitch);
			}
			else
			{
				Globals.Fade.ShowInstant();
			}

			Globals.Subtitles.Hide();
			Globals.Persistence.ResetMemoryVars();
			var currentSceneManager = FindSceneManager(SceneManager.GetActiveScene());
			if (currentSceneManager != null)
			{
				currentSceneManager.Unload();
				RemoveInputHandler(currentSceneManager);
			}
			// Load routine
			Globals.Videos?.StopAll();
			Globals.Videos = null;
			
			if ( _preloadingOperation != null )
			{
				_preloadingOperation.allowSceneActivation = true;
			}
				
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());		
			TransitionCamera.SetActive(true);
			yield return null;
			Resources.UnloadUnusedAssets();
			GC.Collect();

			// Preloading
			if ( _preloadingOperation != null )
			{
				while (!_preloadingOperation.isDone)
				{
					yield return null;
				}
				_preloadingOperation = null;
				if (_preloadedScene != sceneName)
				{
					yield return SceneManager.UnloadSceneAsync(_preloadedScene);
					yield return null;
					yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);	
				}
				_preloadedScene = null;
			}
			else
			{				
				// Load without preload
				yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			}
					
			TransitionCamera.SetActive(false);
			OnSceneLoad?.Invoke();
			yield return null;
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 1; // this might get changed in VideoPool Update
			// Init new scene
			var newlyLoaded = SceneManager.GetSceneByName(sceneName);
			SceneManager.SetActiveScene(newlyLoaded);
			IsLoading = false;
			StartCoroutine(LoadScene(newlyLoaded));
		}
	
		public IEnumerator LoadScene(Scene scene)
		{
			if (!scene.isLoaded)
			{
				Debug.LogError("Scene must first be loaded by unity, then by GameManager");
				yield break;
			}
			var sceneManager = FindSceneManager(scene);
			if (sceneManager != null)
			{
				if (!sceneManager.UntrackedScene)
				{
					Globals.Persistence.SaveProfile(scene.name);
				}
				
				if (!sceneManager.Loaded)
				{
					yield return sceneManager.Load();
				}

				RegisterInputHandler(sceneManager, 1);
			}
			_inputEnabled = true;
		}

		private CEScene FindSceneManager(Scene scene)
		{
			var objects = scene.GetRootGameObjects();
			foreach (var o in objects)
			{
				var sceneManager = o.GetComponent<CEScene>();
				if (sceneManager != null)
				{
					return sceneManager;
				}
			}
			return null;
		}

		#region Input

		// Helper class for storing InputHandlers and priority
#if UNITY_EDITOR
		public class InputHandleRecord
#else
		private class InputHandleRecord
#endif
		{
			public readonly IInputHandler Handler;
			public readonly int Priority;

			public InputHandleRecord(IInputHandler handler, int priority)
			{
				Handler = handler;
				Priority = priority;
			}
		}

		private List<InputHandleRecord> _inputHandlers = new List<InputHandleRecord>();
		private List<InputHandleRecord> _tempInputHandlers;

		#if UNITY_EDITOR
		public List<InputHandleRecord> InHandlers => _inputHandlers; 
		#endif
		
		private bool _inputEnabled;
		private bool _inUpdateIteration;
		
		public void RegisterInputHandler(IInputHandler handler, int priority)
		{
			if (!HasInputHandler(handler))
			{
				if (!_inUpdateIteration)
				{
					_inputHandlers.Add(new InputHandleRecord(handler, priority));
					_inputHandlers.Sort((b, a) => a.Priority.CompareTo(b.Priority));
				}
				else
				{
					if (_tempInputHandlers == null)
					{
						_tempInputHandlers = new List<InputHandleRecord>(_inputHandlers);
					}
					_tempInputHandlers.Add(new InputHandleRecord(handler, priority));
					_tempInputHandlers.Sort((b, a) => a.Priority.CompareTo(b.Priority));
				}
			}
		}

		private bool HasInputHandler(IInputHandler handler)
		{
			foreach (var record in _inputHandlers)
			{
				if (record.Handler == handler) return true;
			}
			return false;
		}

		public void RemoveInputHandler(IInputHandler handler)
		{
			if (handler == null) return;
			if (HasInputHandler(handler))
			{
				if (!_inUpdateIteration)
				{
					_inputHandlers.RemoveAll(record => record.Handler == handler);
				}
				else
				{
					if (_tempInputHandlers == null)
					{
						_tempInputHandlers = new List<InputHandleRecord>(_inputHandlers);
					}
					_tempInputHandlers.RemoveAll(record => record.Handler == handler);
				}
			}
		}

		private void Update()
		{
			if (!_inputEnabled) return;
			_inUpdateIteration = true;
			foreach (var record in _inputHandlers)
			{
				if (record.Handler == null)
				{
					Debug.LogError("A registered input handler was destroyed");
					continue;
				}
				if (record.Handler.HandleInput()) break;
			}
			_inUpdateIteration = false;

			if (_tempInputHandlers != null) // in case some handlers were added or removed while we were updating
			{
				_inputHandlers = _tempInputHandlers;
				_tempInputHandlers = null;
			}
		}

		#endregion

		public void PreloadScene(string sceneName)
		{
			StartCoroutine(PreloadSceneRoutine(sceneName));
		}
		
		private IEnumerator PreloadSceneRoutine(string sceneName)
		{
			_preloadedScene = sceneName;
			yield return null;
			_preloadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			_preloadingOperation.allowSceneActivation = false;
		}
	}
}