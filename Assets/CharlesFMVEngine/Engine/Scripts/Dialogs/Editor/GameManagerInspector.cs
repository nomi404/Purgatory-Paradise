using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerInspector : Editor 
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                GUILayout.Space(20);
                GUILayout.Label("InputHandlers:");
                GameManager gm = target as GameManager;
                foreach (var str in gm.InHandlers)
                {
                    GUILayout.Label(str.Priority + " " + ((MonoBehaviour) str.Handler).gameObject.name);
                }
            }
        }
    }
}
