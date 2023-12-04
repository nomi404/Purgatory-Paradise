using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
#pragma warning disable 0649
// Based on a code samples from http://answers.unity.com/answers/1204071/view.html
namespace CharlesEngine
{
    [Serializable]
    public class SceneField
    {
        [SerializeField]
        private Object m_SceneAsset;
        [SerializeField]
        private string m_SceneName = "";
        public string SceneName
        {
            get { return m_SceneName; }
#if UNITY_EDITOR
            set
            {
                var guid = AssetDatabase.FindAssets(value + " t:Scene")[0];
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
                m_SceneAsset = sceneAsset;
                m_SceneName = value;
            }
#else
            set
            {
                m_SceneName = value;
            }
#endif
        }

        // makes it work with the existing methods (LoadLevel/LoadScene)
        public static implicit operator string( SceneField sceneField )
        {
            return sceneField.SceneName;
        }
#if UNITY_EDITOR
        public bool Refresh()
        {
            var orig = SceneName;
            if (m_SceneAsset == null)
            {
                Debug.LogError("Asset is null");
            }else if (!(m_SceneAsset is SceneAsset))
            {
                Debug.LogError("Asset is not scene asset");
            }
            else
            {
                m_SceneName = (m_SceneAsset as SceneAsset).name;
                if (m_SceneName != orig)
                {
                    Debug.Log("Fixing scene reference:"+orig+", new name:"+m_SceneName);
                    return true;
                }
            }
            return false;
        }
#endif        
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, GUIContent.none, _property);
            SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
            _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
            if (sceneAsset != null)
            {
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false); 
                if( sceneAsset.objectReferenceValue != null )
                {
                    sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
                }
            }
            EditorGUI.EndProperty( );
        }
    }
#endif
}