using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Toolbox/Fade Out Tween")]
	public class FadeOutTween : FadeTween, IFadeOutTween {
	
		public bool DisableOnEnd;
	
		public override void InitAlpha()
		{
			//do nothing
		}
	
		protected override void OnTweenComplete()
		{
			if(DisableOnEnd) gameObject.SetActive(false);
			base.OnTweenComplete();
		}
	
		public void Reset()
		{
			TargetAlpha = 0;
			DisableOnEnd = true;
		}
	}
}
