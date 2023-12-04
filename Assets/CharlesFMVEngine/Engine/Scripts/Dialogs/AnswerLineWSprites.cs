using TMPro;
using UnityEngine;
using Uween;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
	// This implementation supports Sprites instead of text for certain keywords
	public class AnswerLineWSprites : MonoBehaviour, IAnswerLine
	{
		private AnswerLineEvent _onClickEvent = new AnswerLineEvent();
		[Tooltip("Specify keywords that will be substituted by sprites. Makes sure these two arrays are the same size.")]
		public string[] Keywords;
		[Tooltip("Pair a sprite for a given keyboard. Makes sure the two arrays are the same size.")]
		public Sprite[] Sprites;
		public Node Node { get; set; }
		private SpriteRenderer _spriteRenderer;
		private TextMeshPro _text;
		private BoxCollider2D _collider;
		void Awake()
		{
			_text = GetComponentInChildren<TextMeshPro>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			_collider = GetComponentInChildren<BoxCollider2D>();
			if (Keywords.Length != Sprites.Length)
			{
				Debug.LogError("Answer line: there must be equal amount of keywords and sprites.");
			}
		}
		
#if !UNITY_ANDROID && !UNITY_IOS
		private void OnMouseEnter()
		{
			TweenS.Add(gameObject, 0.1f, 1.05f).EaseInOutSine();
		}

		private void OnMouseExit()
		{
			TweenS.Add(gameObject, 0.1f, 1f).EaseInOutSine();;
		}
#endif
		
#if UNITY_ANDROID || UNITY_IOS
		private void OnMouseDown()
		{
			TweenS.Add(gameObject, 0.1f, 1.05f).EaseInOutSine();
		}
		
		private void OnMouseUp()
		{
			TweenS.Add(gameObject, 0.1f, 1f).EaseInOutSine();;	
		}
#endif
		private void OnMouseUpAsButton()
		{
			Log("answer clicked "+name, DIALOG);

			OnClick?.Invoke(this);
		}

		public void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}

		public void SetText(string text)
		{
			// Search for keywords in the text
			for (var index = 0; index < Keywords.Length; index++)
			{
				var key = Keywords[index];
				
				if (text.Contains(key))
				{
					var sprite = Sprites[index];
					_spriteRenderer.sprite = sprite;
					_text.text = "";
					_collider.size = sprite.rect.size/sprite.pixelsPerUnit; //make the collider fit the sprite
					return;
				}
			}
			
			// No keywords, just use the text
			_spriteRenderer.sprite = null;
			_text.text = text;
			var rt = _text.GetComponent<RectTransform>();
			_collider.size = rt.sizeDelta; //make the collider fit the text
		}

		public AnswerLineEvent OnClick => _onClickEvent;

		public bool IsActive()
		{
			return gameObject.activeSelf;
		}
	}
}