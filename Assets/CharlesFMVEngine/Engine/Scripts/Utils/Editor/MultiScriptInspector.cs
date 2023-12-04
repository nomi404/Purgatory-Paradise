using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(MultiScript))]
    public class MultiScriptInspector : Editor {

// Draw the property inside the given rect
        public override void OnInspectorGUI()
        {		
            MultiScript myTarget = (MultiScript)target;
            var numComponenets = myTarget.gameObject.GetComponents<CEScript>().Length;
            GUILayout.Space(17);
            if (numComponenets <= 1)
            {
                GUILayout.Label(new GUIContent("Add scripts to run   ↓","This script will run all CE scripts added to this game object."), EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label(new GUIContent("Scripts:"+(numComponenets-1),"This script will run all CE scripts added to this game object."), EditorStyles.boldLabel);
            }
            GUILayout.Space(17);
        }
    }
}
