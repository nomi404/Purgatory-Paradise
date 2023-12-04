#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.VersionControl;
using UnityEngine;

namespace CharlesEngine
{
    [InitializeOnLoad]
    class UnityEditorStartupBuildMenuHook {
        static UnityEditorStartupBuildMenuHook() {
            BuildPlayerWindow.RegisterBuildPlayerHandler( // we need to register here to be able to build subtitles AssetBundles
                buildPlayerOptions => {
                    
                    BuildMenu.CheckBuildSettingsScenes();
                    
                    // 1. Build Subtitles Asset Bundles
                    EditorUtility.DisplayProgressBar("Preparing to build", "Assembling subtitles", 0.2f);
                    SubtitlesWindow.BuildAssetBundle(buildPlayerOptions.target);
                    
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                    EditorUtility.ClearProgressBar();
                });
        }
    }
    
    public class BuildMenu : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!PreBuildTest())
            {
                throw new Exception("some tests have not passed!");
            }

            // 2. Refresh Variable Managers
            EditorUtility.DisplayProgressBar("Preparing to build", "Refreshing variables", 0.3f);
            RefreshVariables();
            EditorUtility.ClearProgressBar();
        }

        public static void CheckBuildSettingsScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var list = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < scenes.Length; i++)
            {
                if (scenes[i] != null && !string.IsNullOrEmpty(scenes[i].path))
                {
                    list.Add(scenes[i]);
                }
            }

            if (list[0].path.Contains("Master"))
            {
                var tmp = list[0];
                list[0] = list[1];
                list[1] = tmp;
            }
            
            EditorBuildSettings.scenes = list.ToArray();
        }

        private static bool PreBuildTest()
        {
            var guids = AssetDatabase.FindAssets("t:BuildTest");
            if (guids.Length == 0) return true;
            Debug.Log("running " + guids.Length + " tests");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var test = AssetDatabase.LoadAssetAtPath<BuildTest>(path);
                if (!test.RunTest()) return false;
            }
            return true;
        }
        
        private static void RefreshVariables()
        {
            var allVMguids = AssetDatabase.FindAssets("t:VariableManager");
            foreach (var guid in allVMguids)
            {
                var vmpath = AssetDatabase.GUIDToAssetPath(guid);
                var vm = AssetDatabase.LoadAssetAtPath<VariableManager>(vmpath);
                vm.RefreshList();
                foreach( var v in vm.Variables )
                {
                    if (v == null)
                    {
                        throw new Exception("variable manager contains invalid references");
                    }

                    if (string.IsNullOrEmpty(v.Guid))
                    {
                        throw new Exception("variable manager contains variables with no guid "+v.GetType().Name);
                    } 
                }
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

    }
}
#endif