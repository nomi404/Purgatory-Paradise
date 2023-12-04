using System.Collections;
using TMPro;
using UnityEngine;

namespace CharlesEngine
{
	public class TextCollider : MonoBehaviour
	{
		public BoxCollider2D Collider;

		public TextMeshPro Text;
	
		void Start ()
		{
			StartCoroutine(RefreshCollider());
		}

		public IEnumerator RefreshCollider()
		{
			yield return null;
			yield return null;
			if (!gameObject.activeInHierarchy)
			{
				yield break;
			}
			if (Collider == null || Text == null)
			{
				Debug.LogError("TextCollider does not have text or collider!"+name, gameObject);
				yield break;
			}
			Collider.size = new Vector2(Text.textBounds.size.x, Collider.size.y);
		}
	}
}
