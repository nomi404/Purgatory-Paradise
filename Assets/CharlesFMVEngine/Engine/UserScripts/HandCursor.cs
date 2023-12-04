using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Toolbox/Hand Cursor")]
	public class HandCursor : MonoBehaviour {

		public static Texture2D CursorTexture;
		private bool _mouseOn;
		void Awake()
		{
			if (!GetComponent<Collider2D>())
			{
				gameObject.AddComponent<BoxCollider2D>();
			}
			if (CursorTexture == null)
			{
				CursorTexture = Resources.Load<Texture2D>("mouse_cursor");
			}
		}

		void OnMouseEnter()
		{
			_mouseOn = true;
			Cursor.SetCursor (CursorTexture, new Vector2(13,6), CursorMode.Auto);
		}
     
		void OnMouseExit()
		{	
			_mouseOn = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}

		private void OnDisable()
		{
			if (_mouseOn)
			{
				_mouseOn = false;
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}

		public void Disable()
		{
			if (_mouseOn)
			{
				_mouseOn = false;
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
			Destroy(this);
		}
	}
}
