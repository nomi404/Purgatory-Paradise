using UnityEngine;

namespace CharlesEngine
{
	public abstract class SceneMainScript : MonoBehaviour, IInputHandler {

		protected void Start()
		{
			var sceneManager = FindObjectOfType<CEScene>();
			if (sceneManager == null)
			{
				Debug.LogError("No CEScene found, add script CEScene to the scene.");
				return;
			}
		
			if (sceneManager.Loaded)
			{
				StartScene();
			}
			else
			{
				sceneManager.OnScreenShow.AddListener( OnStartScene );
			}
			sceneManager.OnScreenHide.AddListener( OnEndScene );
		}
	
		private void OnStartScene()
		{
			Globals.GameManager.RegisterInputHandler(this,2);
			StartScene();
		}
	
		private void OnEndScene()
		{
			Globals.GameManager.RemoveInputHandler(this);
			//ScreenEnd();
		}

		public abstract void Prepare();

		protected abstract void StartScene();
	
		public virtual bool HandleInput()
		{
			//override
			return false;
		}
	}
}
