using UnityEditor;
using UnityEngine;

#pragma warning disable 0649 
namespace CharlesEngine
{
	public class Globals : MonoBehaviour
	{
		public static bool Loaded;

		public static string Lang;
	
		public static GameObject VideosPrefab;
		public static VideoPool Videos;
		public static SoundPool Sounds;
		public static DialogChoices Choices;
		public static SubtitlesManager Subtitles;
		public static VariablePersistence Persistence;
		public static GameManager GameManager;
		public static GameInput Input;
		public static FadeManager Fade;
		public static PauseMenuManager Pause;
		public static CEngineSettings Settings;
		public static CEUtils Utils;

		[SerializeField] private GameObject _videos;
		[SerializeField] private SoundPool _sounds;
		[SerializeField] private DialogChoices _choices;
		[SerializeField] private SubtitlesManager _subtitles;
		[SerializeField] private FadeManager _fade;
		[SerializeField] private PauseMenuManager _pause;
		[SerializeField] private CEngineSettings _settings;

		public void SetSettings(CEngineSettings s)
		{
			_settings = s;
		}
		
		void Awake ()
		{
			Loaded = true;
			VideosPrefab = _videos;
			Sounds = _sounds;
			Subtitles = _subtitles;
			Choices = _choices;
			Fade = _fade;
			Pause = _pause;
			Persistence = GetComponent<VariablePersistence>();
			GameManager = GetComponent<GameManager>();
			Input = GetComponent<GameInput>();
			Utils = gameObject.AddComponent<CEUtils>();
			Settings = _settings;
#if UNITY_EDITOR
			if (Settings == null)
			{
				// Attemp to load from resources
				Settings = Resources.Load<CEngineSettings>("CEngineSettings");
				if (Settings == null)
				{
					// Create new scriptable object and save it
					var asset = ScriptableObject.CreateInstance<CEngineSettings>();
					AssetDatabase.CreateAsset(asset, "Assets/Resources/CEngineSettings.asset");
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					Settings = asset;
					Debug.LogWarning("Creating a new CEngineSettings in Resources", asset);
				}
			}
#endif
			if (Settings != null)
			{
				if (Settings.GlobalsPluginsPrefab != null)
				{
					var customGlobals = Instantiate(Settings.GlobalsPluginsPrefab, transform);
					customGlobals.name = "CustomGlobals";
				}

				ScreenScaler.ReferenceResolution = Settings.Resolution;
				if (Settings.Languages != null && Settings.Languages.Length > 0)
				{
					Lang = Settings.Languages[0];
				}

				if (string.IsNullOrEmpty(Lang))
				{
					Lang = "en";
				}
			}
			else
			{
				Debug.LogError("Settings reference is missing in the Master scene. Try deleting CESettings in resources.");
				Settings = ScriptableObject.CreateInstance<CEngineSettings>();
				Lang = "en";
			}
			
#if CE_USE_I2Loc
			Lang = I2.Loc.LocalizationManager.CurrentLanguageCode;
			Debug.Log("setting lang " + Lang);
#endif
			
			Subtitles.Init();
		}

		public static void Unload()
		{
			Subtitles.UnloadBundles();
			Loaded = false;
			Videos = null;
		}
	}
}
