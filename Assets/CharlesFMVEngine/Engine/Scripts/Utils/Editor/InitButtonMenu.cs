using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	public class InitButtonMenu : MonoBehaviour
	{
		[MenuItem("Tools/Charles Engine/Actions/Init Button %#E", false, 51)]
		private static void InitButton()
		{
			var selected = (GameObject) Selection.activeObject;
			if (selected == null)
			{
				return;
			}
			if( selected.GetComponent<SpriteRenderer>() == null )
				selected.AddComponent<SpriteRenderer>();
			
			if( selected.GetComponent<Collider2D>() == null )
				selected.AddComponent<BoxCollider2D>();
			
			selected.AddComponent<HandCursor>();
			selected.AddComponent<EventListener>();
		}

		[MenuItem("Tools/Charles Engine/Init Button %#E", true)]
		private static bool NewMenuOptionValidation()
		{
			return Selection.activeObject != null;
		}

		/*[MenuItem("Tools/Charles Engine/Arrange/Distribute horizontally", false, 51)]
	private static void DistributeH()
	{
		var selected = Selection.gameObjects;
		if (selected == null || selected.Length < 2)
		{
			return;
		}

		Array.Sort(selected, (a,b) => a.transform.position.x.CompareTo(b.transform.position.x));
		
		var min = selected[0].transform.position.x;
		var max = selected[selected.Length - 1].transform.position.x;
		
		var d = (max - min) / (selected.Length - 1);

		for (var i = 0; i < selected.Length; i++)
		{
			if (selected[i] == null) return;
			var p = selected[i].transform.position;
			p.x = min + i * d;
			selected[i].transform.position = p;
		}
	}*/
	}
}