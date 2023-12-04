using UnityEditor;

namespace CharlesEngine
{
	[CustomEditor(typeof(SimpleScript)), CanEditMultipleObjects]
	public class SimpleScriptInspector : Editor
	{
		public SerializedProperty EventProperty;

		void OnEnable()
		{
			EventProperty = serializedObject.FindProperty("Event");
		}

		public override void OnInspectorGUI()
		{
			if (targets != null && targets.Length > 1)
			{
				EditorGUILayout.PropertyField(EventProperty);
				serializedObject.ApplyModifiedProperties();
				foreach (var target in targets)
				{
					//((SimpleEvent) target ).Event = EventProperty. // TODO multiobject editing
				}
			}
			else
			{
				EditorGUILayout.PropertyField(EventProperty);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}