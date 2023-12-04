using System;
using UnityEngine;

namespace CharlesEngine
{
	public class GameInput : MonoBehaviour
	{
		public InputMapping GameInputMapping;
		private GestureDetector _gestureDetector = new GestureDetector();
		public bool GetKeyDown(InputAction action)
		{
			return Input.GetKeyDown( GameInputMapping[action] );
		}
	
		public bool GetKeyUp(InputAction action)
		{
#if UNITY_IOS || UNITY_ANDROID
			if (action == InputAction.PauseGame && _gestureDetector.EscapeMenuGesture)
			{
				return true;
			}
			if (action == InputAction.SkipVideo && _gestureDetector.SkipVidGesture)
			{
				return true;
			}
#endif
			return Input.GetKeyUp( GameInputMapping[action] );
		}

		private void Update()
		{
			_gestureDetector.Update();
		}
	}
}
