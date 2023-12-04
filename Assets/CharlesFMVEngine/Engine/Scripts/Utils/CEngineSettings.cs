using TMPro;
using UnityEngine;

namespace CharlesEngine
{
    //[CreateAssetMenu(fileName = "CEngineSettings", menuName = "CEngineSettings")]// this asset should be created automatically
    public class CEngineSettings : ScriptableObject
    {
        public bool UseI2Localization;
        public GameObject PauseMenuPrefab;
        public GameObject DialogChoicePrefab;
        public GameObject GlobalsPluginsPrefab;
        public string[] Languages;
        public Vector2Int Resolution = new Vector2Int(1920, 1080);
        public TMP_FontAsset DefaultFont;
        public float ForkOverlayAlpha;
    }
}