using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
    public class CharlesEngineWindow : EditorWindow
    {
        private static CEngineSettings Settings;
        private Texture2D Image;
        
        
        [MenuItem("Tools/Charles Engine/About", false, 461)]
        public static void ShowWindow()
        {

#if UNITY_2021_1    //temp workaround unity bug
           CreateInstance<CharlesEngineWindow>().Show();
#else
            var w = GetWindow<CharlesEngineWindow>(true, "Charles Engine", true);
            w.minSize = new Vector2(400,500);
#endif
        }

        private void OnGUI()
        {
            GUILayout.Label("Charles Engine v1.09");
            if (Image == null)
            {
                Image = new Texture2D( 1, 1 );
                var guids = AssetDatabase.FindAssets("header t:sprite");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Engine") && path.Contains("Editor"))
                    {
                        Image.LoadImage( System.IO.File.ReadAllBytes( path ) );
                        Image.Apply();
                        break;
                    }
                }
            }
            GUILayout.Box(Image);
            if (Settings == null)
            {
                Settings = Resources.Load<CEngineSettings>("CEngineSettings");
            }

            if (Settings == null)
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    EditorGUILayout.HelpBox("Waiting for compilation to finish...", MessageType.Warning);
                    GUI.enabled = false;
                }
                else
                {
                    EditorGUILayout.HelpBox("REQUIRES SETUP", MessageType.Warning);
                }

                if (GUILayout.Button("INITIALIZE", GUILayout.Height(50)))
                {
                    DoSetup();
                }

                GUI.enabled = true;
            }else if (Settings.DialogChoicePrefab == null || Settings.PauseMenuPrefab == null)
            {
                if (GUILayout.Button("Fix settings", GUILayout.Height(50)))
                {
                    DoSetup();
                }
            }
            if (GUILayout.Button("Documentation", GUILayout.Height(50)))
            {
                Application.OpenURL("https://charlesgames.net/charles-engine/documentation/");
            }
            
            if (Settings != null)
            {
                if (GUILayout.Button("Open Settings", GUILayout.Height(50)))
                {
                    Selection.activeObject = Settings;
                }
                GUILayout.Label("Initialized.");
            }
        }

        private void DoSetup()
        {
            EditorUtility.DisplayProgressBar("Charles Engine Init", "Settings", 0.2f);
            InitSettings();
            InitLogos();
            EditorUtility.DisplayProgressBar("Charles Engine Init", "Layers", 0.4f);
            InitLayers();
            EditorUtility.DisplayProgressBar("Charles Engine Init", "Master scene", 0.6f);
            InitMasterScene();
            EditorUtility.ClearProgressBar();
        }

        private void InitLogos()
        {
            Sprite LogoSprite = null;
            var guids = AssetDatabase.FindAssets("cg_logo_splash t:sprite");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Engine"))
                {
                    LogoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    break;
                }
            }

            if (LogoSprite == null) return;
            var logos = PlayerSettings.SplashScreen.logos;
            var list = new List<PlayerSettings.SplashScreenLogo>(logos);
            foreach( var l in list )
            {
                if (l.logo == LogoSprite) return;
            }
            var lg = new PlayerSettings.SplashScreenLogo();
            lg.logo = LogoSprite;
            lg.duration = 2;
            list.Add(lg);
            PlayerSettings.SplashScreen.logos = list.ToArray();
        }

        private void InitMasterScene()
        {
            if (!EditorBuildSettings.scenes.Any(a => a.path.Contains("Master") ))
            {
                var guids = AssetDatabase.FindAssets("Master t:scene");
                foreach(var guid in guids )
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Engine"))
                    {
                        var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(path,true));
                        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
                    }
                }
            } 
        }

        private void InitSettings()
        {
            Settings = Resources.Load<CEngineSettings>("CEngineSettings");
            if (Settings == null)
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    GUILayout.Label("Waiting for compilation to finish...");
                }

                var asset = CreateInstance<CEngineSettings>();
                // Make sure directory exists
                FileInfo fileInfo = new FileInfo("Assets/Resources/CEngineSettings.asset");
                if (!fileInfo.Exists && fileInfo.Directory != null)
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                AssetDatabase.CreateAsset(asset, "Assets/Resources/CEngineSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Settings = asset;
                Debug.Log("Creating a new CEngineSettings object in Resources", asset);
            }

            if (Settings.PauseMenuPrefab == null)
            {
                var guids = AssetDatabase.FindAssets("PauseMenuDefault t:prefab");
                if (guids.Length == 0)
                {
                    Debug.LogWarning("PauseMenu Prefab not found");
                }
                else
                {
                    Settings.PauseMenuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (Settings.DialogChoicePrefab == null)
            {
                var guids = AssetDatabase.FindAssets("ChoiceLayoutDefault t:prefab");
                if (guids.Length == 0)
                {
                    Debug.LogWarning("Choice Layout Prefab not found");
                }
                else
                {
                    Settings.DialogChoicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            // Assign the new settings to master scene
            var sceneguids = AssetDatabase.FindAssets("Master t:scene");
            foreach (var guid in sceneguids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Engine"))
                {
                    var openScenePah = EditorSceneManager.GetActiveScene().path;
                    
                    var masterscene = EditorSceneManager.OpenScene(path); // load master scene and fix reference to settings
                    GameObject[] rootObjects = masterscene.GetRootGameObjects();
                    foreach (GameObject rootObject in rootObjects)
                    {
                        var globals = rootObject.GetComponentInChildren<Globals>();
                        if (globals != null)
                        {
                            globals.SetSettings(Settings);
                            EditorSceneManager.SaveScene(masterscene);
                        }
                    }

                    EditorSceneManager.OpenScene(openScenePah); // open back the previously loaded scene
                    break;
                }
            }

            EditorUtility.SetDirty(Settings);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private void InitLayers()
        {
            string[] SortingLayers = {"Video", "DialogChoices", "AboveVideo", "FadeOverlay", "PauseMenu", "AbovePauseMenu"};
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("Can't set up the layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
                Debug.LogWarning("Layers is null: " + (layers == null));
                return;
            }
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(28);
            if (layerSP.stringValue != "NotRendered")
            {
                layerSP.stringValue = "NotRendered";
            }

            layerSP = layers.GetArrayElementAtIndex(29);
            if (layerSP.stringValue != "MasterOnly")
            {
                layerSP.stringValue = "MasterOnly";
            }
            
            SerializedProperty sortingLayersProp = tagManager.FindProperty("m_SortingLayers");
            if (sortingLayersProp.arraySize <= 1)
            {
                for (int i = 0; i < SortingLayers.Length; i++)
                {
                    sortingLayersProp.InsertArrayElementAtIndex(sortingLayersProp.arraySize);
                    var newlayer = sortingLayersProp.GetArrayElementAtIndex(sortingLayersProp.arraySize-1);
                    newlayer.FindPropertyRelative("uniqueID").intValue = 5*i + 6;
                    newlayer.FindPropertyRelative("name").stringValue = SortingLayers[i];
                }
            }

            tagManager.ApplyModifiedProperties();
        }
    }


    [InitializeOnLoad]
    public class InitCheck //to check if we need to initializes
    {
        private static CEngineSettings Settings;
        private static GameObject _lastGameObject;
        static InitCheck()
        {
            if (Settings == null)
            {
                // Attemp to load from resources
                Settings = Resources.Load<CEngineSettings>("CEngineSettings");
                if (Settings == null)
                {
#if UNITY_2021_1
                Debug.Log("Please initilize engine in Toole/Charles Engine/About");
#else
                    CharlesEngineWindow.ShowWindow();
#endif
                }
            }
            
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        
        private static void OnHierarchyChanged()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                var target = Selection.activeGameObject;
                if (!target || target == _lastGameObject) return;
                _lastGameObject = target;
                if (target.GetComponent<SpriteRenderer>())
                {
                    if (target.transform.parent != null) return;
                    var root = GameObject.FindObjectOfType<CESceneRoot>();
                    if (root == null) return;
                    target.transform.parent = root.transform;
                    target.transform.localScale = Vector3.one;
                }
            }
        }
    }
}