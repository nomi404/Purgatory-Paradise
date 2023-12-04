using UnityEngine;
using UnityEngine.Events;
using Uween;

namespace CharlesEngine
{
	[ExecuteInEditMode]
	public abstract class FadeTween : CEScript
	{
		// PUBLIC
		[Range(0,1f)]
		public float TargetAlpha = 1;
		[Range(0.05f,8f)]
		public float Duration = 1;
		
		public UnityEvent OnTweenCompleteEvent;

		public bool RunOnStart = false;
	
		// PRIVATE 
		protected SpriteRenderer _renderer;
		protected AlphaGroup _alphaGroup;
	
		protected bool _inited;
	
		protected override void Start ()
		{
            base.Start();
			if (Application.isPlaying)
			{
				if (_renderer == null && _alphaGroup == null)
				{
					InitComponents();
				}

				if (!_inited )
				{
					InitAlpha();
				}

				if (RunOnStart)
				{
					Run();
				}
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			_renderer = GetComponent<SpriteRenderer>();
			_alphaGroup = GetComponent<AlphaGroup>();
			if (_renderer == null && _alphaGroup == null)
			{
				gameObject.AddComponent<AlphaGroup>();
			}
		}
#endif
		protected void InitComponents()
		{
			_renderer = GetComponent<SpriteRenderer>();
			_alphaGroup = GetComponent<AlphaGroup>();
			if (_renderer == null && _alphaGroup == null)
			{
				Debug.LogError("FadeTween could not find component to fade. Must have either SpriteRenderer or AlphaGroup |"+name, gameObject);
			}
		}

		public abstract void InitAlpha();
	
		protected void SetAlpha(float a)
		{
			_inited = true;
			if (_renderer != null)
			{
				var clr = _renderer.color;
				clr.a = a;
				_renderer.color = clr;
			}
			if( _alphaGroup != null )
			{
				_alphaGroup.SetA(a);
			}
		}

		public override void Run()
		{
			if (!_inited)
			{
				InitAlpha();
			}

			gameObject.PauseTweens();
			
			if (_renderer)
			{
				TweenA.Add(gameObject, Duration, TargetAlpha).EaseInOutSine().Then(OnTweenComplete);
			//	_tweener = _renderer.DOFade(TargetAlpha, Duration);
			}
			if (_alphaGroup)
			{
				TweenAG.Add(gameObject, Duration, TargetAlpha).EaseInOutSine().Then(OnTweenComplete);
		//		_tweener = DOTween.To(() => _alphaGroup.Alpha, a => _alphaGroup.SetA(a), TargetAlpha, Duration);
			}
		}

		protected virtual void OnTweenComplete()
		{
			OnTweenCompleteEvent?.Invoke();
		}
	}
}
