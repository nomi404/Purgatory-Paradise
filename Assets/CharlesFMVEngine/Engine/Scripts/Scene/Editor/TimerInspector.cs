using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(Timer))]
    public class TimerInspector : Editor
    {
        private GUIStyle _activestyle;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            if (_activestyle == null)
            {
                _activestyle = new GUIStyle();
                _activestyle.font = EditorStyles.standardFont;
                _activestyle.normal.textColor = Color.red;
                _activestyle.stretchWidth = true;
                _activestyle.alignment = TextAnchor.MiddleCenter;
            }

            if (Application.isPlaying)
            {
                Timer t = (Timer) target;
                if (t.IsActive())
                {
                    GUILayout.Label("ACTIVE", _activestyle);
                }
                else
                {
                    GUILayout.Label("inactive");
                }
            }
        }
        
        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
}