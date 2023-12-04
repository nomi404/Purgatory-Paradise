using UnityEngine;

namespace CharlesEngine
{
	public interface IFadeInTween
	{
		void Run();
	}
	public interface IFadeOutTween
	{
		void Run();
	}
	
	[AddComponentMenu("CE Toolbox/CE GameObject")]
	public class CEGameObject : MonoBehaviour {

		[Header("Appearance")]
		public bool InitialVisibility = true;

		private IFadeInTween _tweenFadeIn;
		private IFadeOutTween _tweenFadeOut;
	
		private void Awake()
		{
			_tweenFadeIn = GetComponent<IFadeInTween>();
			_tweenFadeOut = GetComponent<IFadeOutTween>();
		}

		public void Show()
		{
			gameObject.SetActive(true);
			if (_tweenFadeIn != null) _tweenFadeIn.Run();
		}
	 
		public void Hide()
		{
			if (_tweenFadeOut != null)
			{
				_tweenFadeOut.Run();
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		public void HideAllChildren()
		{
			for (var i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				var comp = child.GetComponent<CEGameObject>();
				if (comp != null) comp.Hide();
			}
		}
	}
}
