using TMPro;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	public class AddTextMenu : MonoBehaviour
	{
		[MenuItem("Tools/Charles Engine/Actions/Add Text %#t", false, 52)]
		private static void AddTextObject()
		{
			var selected = (GameObject) Selection.activeObject;
			var textObj = new GameObject("Text");
			if (selected != null)
			{
				textObj.transform.parent = selected.transform;
			}
			textObj.transform.localPosition = Vector3.zero;
			textObj.transform.localScale = Vector3.one;
			var txOjb = textObj.AddComponent<TextMeshPro>();
			var settings = Resources.Load<CEngineSettings>("CEngineSettings");
			if (settings != null)
			{
				txOjb.font = settings.DefaultFont;
			}
			txOjb.isOrthographic = true;
			txOjb.rectTransform.sizeDelta = new Vector2(200, 100);
			txOjb.enableWordWrapping = false;
			txOjb.isOrthographic = true;
			txOjb.fontSize = 24;
			txOjb.text = "Text";
			txOjb.alignment = TextAlignmentOptions.Top;
#if CE_USE_I2Loc
			textObj.AddComponent<I2.Loc.Localize>();
#endif
			Selection.activeTransform = textObj.transform;
		}
 
		[MenuItem("Tools/Charles Engine/Add Text %#t", true)]
		private static bool NewMenuOptionValidation()
		{
			return Selection.activeObject != null;
		}
	}
}