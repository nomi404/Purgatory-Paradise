using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	public class FindByGuidWindow : EditorWindow
	{

		private string filterText;
	
		[MenuItem("Assets/Find by guid", false, 201)]
		public static void ShowWindow()
		{
			GetWindow<FindByGuidWindow>(true, "Find by GUID", true);
		}

		private void OnGUI()
		{
			filterText = EditorGUILayout.TextField("GUID", filterText);
			if (GUILayout.Button("Find"))
			{
				var path = AssetDatabase.GUIDToAssetPath(filterText);
				Selection.activeObject=AssetDatabase.LoadMainAssetAtPath(path);
			}
		}
	}
}
