using UnityEngine;

namespace CharlesEngine
{
	[RequireComponent(typeof(SpriteRenderer))]
	[AddComponentMenu("CE Toolbox/Changable Sprite")]
	public class ChangableSprite : MonoBehaviour {

		public void ChangeSpriteTo(Sprite sprite)
		{
			GetComponent<SpriteRenderer>().sprite = sprite;
		}
	}
}
