using UnityEditor;

namespace CharlesEngine
{
    [CustomEditor(typeof(PlaySound))]
    public class PlaySoundInspector : Editor {

        public override void OnInspectorGUI()
        {		
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Clip"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Volume"), true);
            EditorGUILayout.Space();
            PlaySound myTarget = (PlaySound)target;
            myTarget.IsSoundEffect = !EditorGUILayout.BeginToggleGroup("Music Settings", !myTarget.IsSoundEffect);
            
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Delay"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HasSubtitles"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Loop"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSoundEnd"), true);
            
            
            EditorGUILayout.EndToggleGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
