using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	[CustomEditor(typeof(CESceneRoot))]
	public class SceneRootInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			if(GUILayout.Button("Set all to initial visibility"))
			{
				((CESceneRoot)target).SetVisibilityToInitial();
			}
		}
	}
}
