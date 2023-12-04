using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(DialogManager))]
    public class DialogManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DialogManager myTarget = (DialogManager)target;
            if (myTarget == null) return;
            if (myTarget.Dialogues != null && myTarget.Dialogues.TextMode)
            {
                if (!(myTarget is TextDialogManager))
                {
                    EditorGUIUtility.labelWidth = 20;
                    EditorGUILayout.HelpBox("Dialog Manager needs to be replaced by TextDialogManager in order to play dialogues in Text Mode.", MessageType.Warning);
                    if (GUILayout.Button("Replace automatically"))
                    {
                        myTarget.SwitchToTextMode();
                    }
                    EditorGUIUtility.labelWidth = 0;
                }
            }
        }
    }
}