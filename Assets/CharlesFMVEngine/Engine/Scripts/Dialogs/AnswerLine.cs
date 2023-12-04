using TMPro;
using UnityEngine;
using Uween;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
	// You can create your own implementation of IAnswerLine to achieve a unique dialog choice effect
	// The below is the default implementation.
	public class AnswerLine : MonoBehaviour, IAnswerLine
	{
		private AnswerLineEvent _onClickEvent = new AnswerLineEvent();
	
		public Node Node { get; set; }

		private TextMeshPro _text;
		void Awake()
		{
			_text = GetComponentInChildren<TextMeshPro>();
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
			_text.text = text;
		}

		public AnswerLineEvent OnClick => _onClickEvent;

		public bool IsActive()
		{
			return gameObject.activeSelf;
		}
	}
}