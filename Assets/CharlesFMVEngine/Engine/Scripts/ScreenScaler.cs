using System;
using UnityEngine;

namespace CharlesEngine
{
	[ExecuteInEditMode]
	public class ScreenScaler : MonoBehaviour {
	
		public static Vector2 ReferenceResolution = new Vector2(1920,1080); //set from CEngineSettings in Globals

		private Vector2 _lastsize;

		private void Awake()
		{
			Resize();
		}

		private void Resize() {
//			print("Screensize: "+Screen.width+"x"+Screen.height);
			var ratio = Screen.width / (float) Screen.height;
			if (ratio < ReferenceResolution.x/ReferenceResolution.y)
			{
				var scalenow = ReferenceResolution.y / Screen.height;
				var scalex = Screen.width / ReferenceResolution.x;
				var sc = scalex * scalenow;
				transform.localScale = new Vector3(sc, sc, sc);
			}
			else
			{
				transform.localScale = new Vector3(1, 1, 1);
			}
			_lastsize = new Vector2(Screen.width, Screen.height);
		}
		
		void Update () {
			if (_lastsize != new Vector2(Screen.width, Screen.height))
			{
				Resize();
			}
		}
	}
}
