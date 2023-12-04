using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	[CustomEditor(typeof(ConditionalScript))]
	public class ConditionalScriptInspector : UnityEditor.Editor {

// Draw the property inside the given rect
		public override void OnInspectorGUI()
		{		
			ConditionalScript myTarget = (ConditionalScript)target;
			if (myTarget.Condition == null)
			{
				myTarget.Condition = new Condition();
				Debug.Log("condition:"+myTarget.Condition+"|");
			}
			GUILayout.Label("IF (Condition):",EditorStyles.boldLabel);
			var guiStyle = new GUIStyle();
			guiStyle.alignment = TextAnchor.MiddleCenter;
			guiStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label(myTarget.Condition.ToString(), guiStyle );
			GUILayout.Space(5);
			if(GUILayout.Button("edit"))
			{
				ConditionEditor window = (ConditionEditor) EditorWindow.GetWindow(typeof(ConditionEditor), false, "Condition Editor");
				window.Condition = myTarget.Condition ?? new Condition();
				window.Show();
			}
			GUILayout.Space(10);
		
			SerializedProperty eevnt = serializedObject.FindProperty("Event"); // <-- UnityEvent
			GUILayout.Label("THEN",EditorStyles.boldLabel);
//		EditorGUIUtility.LookLikeControls();
			EditorGUILayout.PropertyField(eevnt);
			
			if(GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
	
	[CustomEditor(typeof(IfElseScript))]
	public class IfElseConditionalScriptInspector : UnityEditor.Editor {

// Draw the property inside the given rect
		public override void OnInspectorGUI()
		{		
			ConditionalScript myTarget = (ConditionalScript)target;
			if (myTarget.Condition == null)
			{
				myTarget.Condition = new Condition();
				Debug.Log("condition:"+myTarget.Condition+"|");
			}
			GUILayout.Label("IF (Condition):",EditorStyles.boldLabel);
			var guiStyle = new GUIStyle();
			guiStyle.alignment = TextAnchor.MiddleCenter;
			guiStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label(myTarget.Condition.ToString(), guiStyle );
			GUILayout.Space(5);
			if(GUILayout.Button("edit"))
			{
				ConditionEditor window = (ConditionEditor) EditorWindow.GetWindow(typeof(ConditionEditor), false, "Condition Editor");
				window.Condition = myTarget.Condition ?? new Condition();
				window.Show();
			}
			GUILayout.Space(10);
		
			SerializedProperty eevnt = serializedObject.FindProperty("Event"); // <-- UnityEvent
			GUILayout.Label("THEN",EditorStyles.boldLabel);
//		EditorGUIUtility.LookLikeControls();
			EditorGUILayout.PropertyField(eevnt);
			
			SerializedProperty elevnt = serializedObject.FindProperty("Else"); // <-- UnityEvent
			GUILayout.Label("ELSE",EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(elevnt);
			if(GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
