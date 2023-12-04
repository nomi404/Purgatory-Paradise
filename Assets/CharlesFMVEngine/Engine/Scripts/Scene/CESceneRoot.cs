using UnityEngine;

namespace CharlesEngine
{
	public class CESceneRoot : MonoBehaviour {
	
		void Awake()
		{
			SetVisibilityToInitial();
		}

		public void SetVisibilityToInitial()
		{
			var gameObjects = GetComponentsInChildren<CEGameObject>(true);
			for (var index = 0; index < gameObjects.Length; index++)
			{
				var go = gameObjects[index];
				var condition = go.GetComponent<ObjectVisibleIf>();
				if (condition)
				{
					go.gameObject.SetActive(condition.Condition.Eval());
				}
				else
				{
					go.gameObject.SetActive(go.InitialVisibility);
				}
			}
		}
	}
}
