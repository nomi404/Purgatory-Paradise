using UnityEngine;

namespace CharlesEngine
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteToggle : MonoBehaviour
	{
		public Sprite OffSprite;
		public Sprite OnSprite;
		private SpriteRenderer _renderer;

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
		}

		public void ToggleTo(bool value)
		{
			_renderer.sprite = value ? OnSprite : OffSprite;
		}

		private void Reset()
		{
			_renderer = GetComponent<SpriteRenderer>();
			if (_renderer != null)
			{
				OnSprite = _renderer.sprite;
			}
		}
	}
}