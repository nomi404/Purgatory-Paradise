using System;
using UnityEngine;
using Uween;

namespace CharlesEngine
{
	public class FadeManager : MonoBehaviour
	{
		public const float OverlayAlpha = 0.8f; 
		public class FadeType
		{
			public static readonly FadeType SceneSwitch = new FadeType(1.5f);
			public static readonly FadeType Other = new FadeType(0.5f);
			public static readonly FadeType Quick = new FadeType(0.08f);
			public float TweenDuration { get; }

			public FadeType(float dur)
			{
				TweenDuration = dur;
			}
		}

		public GameObject Black;

		private SpriteRenderer _blackSprite;
		private Color _transparentBlack;

		private void Awake()
		{
			_blackSprite = Black.GetComponent<SpriteRenderer>();
			_transparentBlack = Color.black;
			_transparentBlack.a = 0;
		}

		public void ShowInstant()
		{
			_blackSprite.color = Color.black;
			Black.PauseTweens();
			//_blackSprite.DOKill();
			Black.SetActive(true);
		}

		public void ShowOverlay(float a = OverlayAlpha)
		{
			Black.SetActive(true);
			if( Math.Abs(_blackSprite.color.a - a) < 0.001f) return;
			_blackSprite.color = _transparentBlack;
			Black.PauseTweens();
			TweenA.Add(Black, FadeType.Quick.TweenDuration, a);
			//_blackSprite.DOKill();
			//_blackSprite.DOFade(a, FadeType.Quick.TweenDuration);
		}

		public void HideOverlay()
		{
			_blackSprite.color = _transparentBlack;
			Black.PauseTweens();
			TweenA.Add(Black, FadeType.Quick.TweenDuration, 0).Then(Reset);
			//_blackSprite.DOKill();
			//_blackSprite.DOFade(0,  FadeType.Quick.TweenDuration).OnComplete(Reset);
		}

		public void HideInstant()
		{
			Black.PauseTweens();
			Black.SetActive(false);
		}

		public WaitForSeconds In(FadeType type)
		{
			Black.SetActive(true);
			_blackSprite.color = _transparentBlack;
			Black.PauseTweens();
			TweenA.Add(Black, type.TweenDuration, 1);
			//_blackSprite.DOFade(1, type.TweenDuration);
			return new WaitForSeconds(type.TweenDuration);
		}

		public void In()
		{
			In(FadeType.Other);
		}

		public void Out(FadeType type)
		{
			Black.SetActive(true);
			_blackSprite.color = Color.black;
			Black.PauseTweens();
			TweenA.Add(Black, type.TweenDuration, 0).Then(Reset);
			//_blackSprite.DOKill();
			//_blackSprite.DOFade(0, type.TweenDuration).OnComplete(Reset);
		}

		public void Out()
		{
			Out(FadeType.Other);
		}
	
		private void Reset()
		{
			_blackSprite.color = Color.black;
			Black.SetActive(false);
		}

		public void To(float val)
		{
			if (val > 0)
			{
				Black.SetActive(true);
				_blackSprite.color = new Color(0, 0, 0, val);
			}
			else
			{
				Black.SetActive(false);
			}
		}
	}
}
