using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(CEGameObject))]
    public class CEGameObjectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            CEGameObject t = (CEGameObject) target;
            if (t.gameObject.GetComponent<ObjectVisibleIf>() != null)
            {
                GUI.enabled = false;
            }
            var boo = EditorGUILayout.Toggle("Initial Visibility", t.InitialVisibility);
            if (boo != t.InitialVisibility)
            {
                Undo.RecordObject(target,"initial visibility");
                t.InitialVisibility = boo;
            }
            GUI.enabled = true;
        }
    }
}