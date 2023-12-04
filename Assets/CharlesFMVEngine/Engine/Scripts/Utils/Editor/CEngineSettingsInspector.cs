using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    [CustomEditor(typeof(CEngineSettings))]
    public class CEngineSettingsInspector : Editor
    {
        private const string I2LocFlag = "CE_USE_I2Loc";
        private void OnEnable()
        {
            // Localization
            CEngineSettings myTarget = (CEngineSettings) target;
#if CE_USE_I2Loc
            myTarget.UseI2Localization = true;
#else
            myTarget.UseI2Localization = false;
#endif

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CEngineSettings myTarget = (CEngineSettings) target;
            
            // Localization
            var foundGuids = AssetDatabase.FindAssets("I2Languages t:ScriptableObject");
            if (foundGuids.Length == 0)
            {
                GUI.enabled = false;
            }
            
            if (foundGuids.Length == 0)
            {
                EditorGUILayout.HelpBox("Import the I2 Localization plugin", MessageType.Info);
            }
            else if( !myTarget.UseI2Localization )
            {
                EditorGUILayout.HelpBox("Check to enable I2 Localization integration", MessageType.Info);
            }
            
            var locValue = EditorGUILayout.Toggle(new GUIContent( "Use I2Localization",GUI.enabled ? "Check to include I2 localization options" : "I2 localization plugin not found in the project."), myTarget.UseI2Localization );
            if (locValue != myTarget.UseI2Localization)
            {
                RefreshCompilationFlag(locValue, I2LocFlag);
                myTarget.UseI2Localization = locValue;
            }

            GUI.enabled = true;


            myTarget.PauseMenuPrefab = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Pause Menu Prefab","Add your custom pause menu here or keep the default"), myTarget.PauseMenuPrefab, typeof(GameObject), false);

            myTarget.DialogChoicePrefab = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Dialog Choice Prefab","Add your custom dialog choice prefab here or keep the default"), myTarget.DialogChoicePrefab, typeof(GameObject), false);

            myTarget.GlobalsPluginsPrefab = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Globals Plugins Prefab","Add your custom globals plugins here or leave it empty"), myTarget.GlobalsPluginsPrefab, typeof(GameObject), false);
            
            myTarget.Resolution = EditorGUILayout.Vector2IntField(new GUIContent("Target Resolution","New scenes will be setup with this resolution"), myTarget.Resolution);
            myTarget.DefaultFont = (TMP_FontAsset) EditorGUILayout.ObjectField( new GUIContent("Default Font","Add Text option will use this font"), myTarget.DefaultFont, typeof(TMP_FontAsset), false);

            // Language
            var oldLang = myTarget.Languages;
            if (oldLang == null || oldLang.Length == 0)
            {
                var langs = Enum.GetValues(typeof(LangEnum)).Cast<LangEnum>().ToArray();
                oldLang = new string[langs.Length];
                int i = 0;
                foreach (LangEnum lang in langs)
                {
                    oldLang[i++] = Enum.GetName(typeof(LangEnum), lang);
                }
                myTarget.Languages = oldLang;
            }
            EditorGUI.BeginChangeCheck();
            SerializedProperty property = serializedObject.FindProperty("Languages");
            EditorGUILayout.PropertyField(property, new GUIContent("Languages"), true);
            ValidateInput(myTarget.Languages);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var props = serializedObject.FindProperty("Languages"); // using myTarget.Languages return old version of data
                var list = new List<string>();
                for (int i = 0; i < props.arraySize; i++)
                {
                    list.Add(props.GetArrayElementAtIndex(i).stringValue);    
                }
                var newValue = list.ToArray();
                var isValid = ValidateInput(newValue);
                if (isValid)
                {
                    var scripts = AssetDatabase.FindAssets("LangEnum t:Script");
                    if (scripts.Length == 1)
                    {
                        var scriptsFileGuid = scripts[0];
                        var assetPath = AssetDatabase.GUIDToAssetPath(scriptsFileGuid);
                        File.WriteAllText(assetPath, GetEnumString(newValue));
                        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                        EditorUtility.SetDirty(textAsset);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogWarning("LangEnum not found");
                    }
                }
            }
            // end language
            myTarget.ForkOverlayAlpha = EditorGUILayout.Slider("Fork Overlay Alpha", myTarget.ForkOverlayAlpha, 0, 1);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private bool ValidateInput(string[] langs)
        {
            if (langs.Length == 0)
            {
                EditorGUILayout.HelpBox("There must be at least one language.", MessageType.Error);
                return false;
            }
            foreach (var l in langs)
            {
                if (l.Length != 2)
                {
                    EditorGUILayout.HelpBox("Language must be 2 chararacters", MessageType.Error);
                    return false;
                }
                if (!l.All(char.IsLetter))
                {
                    EditorGUILayout.HelpBox("Language must contain only letters", MessageType.Error);
                    return false;
                }
#if CE_USE_I2Loc
                if ( string.IsNullOrWhiteSpace(I2.Loc.LocalizationManager.GetLanguageFromCode(l)))
                {
                    EditorGUILayout.HelpBox("Language invalid "+l, MessageType.Error);
                    return false;
                }
#endif
            }
            if (langs.Length != langs.Distinct().Count())
            {
                EditorGUILayout.HelpBox("Languages must be unique", MessageType.Error);
                return false;
            }
            return true;
        }

        private string GetEnumString(string[] langs)
        {
            var start = "namespace CharlesEngine\n" +
                   "{\n" +
                   "\tpublic enum LangEnum{\n";
            var content = "";
            for (int i = 0; i < langs.Length; i++)
            {
                content += "\t"+langs[i] + " = " + i + (i < langs.Length-1 ? "," : "") + "\n";
            }
            return start + content + "\t}\n}\n";
        }

        private void RefreshCompilationFlag(bool value, string flagName)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            
            if (allDefines.Contains(flagName) && !value)
            {
                allDefines.Remove(flagName);
            }else if (!allDefines.Contains(flagName) && value)
            {
                allDefines.Add(flagName);
            }
           
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }
}
