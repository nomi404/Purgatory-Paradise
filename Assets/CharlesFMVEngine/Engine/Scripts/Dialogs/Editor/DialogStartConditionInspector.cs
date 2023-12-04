using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	[CustomEditor(typeof(DialogStartCondition))]
	public class DialogStartConditionInspector : UnityEditor.Editor {

		public override void OnInspectorGUI()
		{		
			DialogStartCondition myTarget = (DialogStartCondition)target;
			if (myTarget.Condition == null)
			{
				myTarget.Condition = new Condition();
			}
			GUILayout.Label("Condition:",EditorStyles.boldLabel);
			GUILayout.Label(myTarget.Condition.ToString(), EditorStyles.centeredGreyMiniLabel );
			if(GUILayout.Button("edit"))
			{
				ConditionEditor window = (ConditionEditor) EditorWindow.GetWindow(typeof(ConditionEditor));
				window.Condition = myTarget.Condition ?? new Condition();
				window.Show();
			}
			GUILayout.Space(20);
			GUILayout.Label("Start Node:",EditorStyles.boldLabel);
			if (myTarget.Dialog == null)
			{
				var dialogManager = myTarget.GetComponent<DialogManager>();
				if (dialogManager != null) myTarget.Dialog = dialogManager.Dialogues;
			}

			if (myTarget.Dialog != null)
			{
				myTarget.TreeIndex = EditorGUILayout.Popup(myTarget.TreeIndex, myTarget.Dialog.TabNames);
				if (myTarget.TreeIndex < myTarget.Dialog.TabNames.Length)
				{
					myTarget.Tree = myTarget.Dialog.TabNames[myTarget.TreeIndex];
				}

				EditorGUIUtility.labelWidth = 20;
				myTarget.Node = EditorGUILayout.TextField("ID", myTarget.Node);
				EditorGUIUtility.labelWidth = 0;
			}

			if(GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
