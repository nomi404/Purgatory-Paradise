using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Toolbox/Fade In Tween")]
	public class FadeInTween : FadeTween, IFadeInTween {
		public override void InitAlpha()
		{
			InitComponents();
			SetAlpha(0);
		}
	}
}
