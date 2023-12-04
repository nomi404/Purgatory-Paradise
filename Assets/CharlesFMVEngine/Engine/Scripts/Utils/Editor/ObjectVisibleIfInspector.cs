using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(ObjectVisibleIf))]
    public class ObjectVisibleIfInspector : Editor {

// Draw the property inside the given rect
        public override void OnInspectorGUI()
        {		
            ObjectVisibleIf myTarget = (ObjectVisibleIf)target;
            if (myTarget.Condition == null)
            {
                myTarget.Condition = new Condition();
                Debug.Log("condition:"+myTarget.Condition+"|");
            }
            GUILayout.Label("Condition:",EditorStyles.boldLabel);
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel );
            style.normal.textColor = Color.black;
            GUILayout.Label(myTarget.Condition.ToString(), style );
            if(GUILayout.Button("edit"))
            {
                ConditionEditor window = (ConditionEditor) EditorWindow.GetWindow(typeof(ConditionEditor));
                window.Condition = myTarget.Condition ?? new Condition();
                window.TargetObject = target;
                Undo.RecordObject(target, "edit condition");
                window.Show();
            }
        }
    }
}
