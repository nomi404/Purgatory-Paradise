

using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(JumpToNode))]
    public class JumpToNodeInspector : UnityEditor.Editor {

        public override void OnInspectorGUI()
        {		
            JumpToNode myTarget = (JumpToNode)target;

            GUILayout.Space(20);
            GUILayout.Label("Tree:",EditorStyles.boldLabel);
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

                EditorGUIUtility.labelWidth = 50;
                myTarget.Node = EditorGUILayout.TextField("Node ID: ", myTarget.Node);
                EditorGUIUtility.labelWidth = 0;
            }

            if(GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}