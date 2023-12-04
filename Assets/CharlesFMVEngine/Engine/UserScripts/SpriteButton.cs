using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Toolbox/SpriteButton")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteButton : MonoBehaviour
	{
		public Sprite Normal;
		public Sprite Highlighted;
		private SpriteRenderer _renderer;

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
			Normal = _renderer.sprite;
			if (Highlighted == null)
			{
				Highlighted = Normal;
			}
		}

		private void OnMouseEnter()
		{
			_renderer.sprite = Highlighted;
		}

		private void OnMouseExit()
		{
			_renderer.sprite = Normal;
		}

		private void Reset()
		{
			_renderer = GetComponent<SpriteRenderer>();
			if (_renderer != null)
			{
				Normal = _renderer.sprite;
			}
		}
	}
}