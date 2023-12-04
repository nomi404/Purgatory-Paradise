using System;
using UnityEngine;

namespace CharlesEngine
{
	public class PauseMenuManager : MonoBehaviour, IInputHandler {
		public bool IsPaused { get; private set; }

		private GameObject _content;
		
		private void Awake()
		{
			gameObject.SetActive(false);
		}

		public void PauseGame(bool showIcons = true )
		{
			//	Time.timeScale = 0;
			Globals.Videos.PauseAll();
			Globals.Sounds.PauseAll();
			
			if (showIcons)
			{
				gameObject.SetActive(true);
			}

			Globals.Fade.ShowOverlay();
		
			IsPaused = true;
			Globals.GameManager.RegisterInputHandler(this,10);

			if (_content == null)
			{
				if (Globals.Settings.PauseMenuPrefab == null)
				{
					throw new Exception("No pause prefab in settings!");	
				}
				_content = Instantiate(Globals.Settings.PauseMenuPrefab, transform);
				var manager = _content.GetComponent<PauseMenuBase>();
				manager.Init();
				manager.OnPauseCancel.AddListener(Resume);
			}
		}
	
		// Only hides pause menu, game remains paused with black overlay
		public void Hide()
		{
			Globals.GameManager.RemoveInputHandler(this);
			gameObject.SetActive(false);
		}

		// Resumes game and hides pause menu and overlay
		public void Resume()
		{
			IsPaused = false;
			Time.timeScale = 1;
			Globals.Videos.Resume();
			Globals.Sounds.Resume();
			Globals.Fade.HideOverlay();
			Hide();
		}

		public bool HandleInput()
		{
			if (!gameObject.activeSelf) return false;

			if ( Globals.Input.GetKeyUp(InputAction.PauseGame) )
			{
				Resume();
			}
			
			return true;
		}
	}
}
