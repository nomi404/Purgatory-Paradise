namespace CharlesEngineSamples
{
    using CharlesEngine;
    using TMPro;
    using UnityEngine;

    public class SampleMainMenuManager : SceneMainScript
    {
        private const string ProfileName1 = "profile_1";
        public SceneField StartingScene;

        public SpriteRenderer PlayButton;
        public Sprite PlayIcon;
        public Sprite ContinueIcon;

        public GameObject LangOptions;
        
        public override void Prepare()
        {
            // Prepare GUI for switching languages
            if (Globals.Settings.Languages.Length > 1)
            {
                if (LangOptions == null) return;
                var pref = LangOptions.transform.GetChild(0);
                var i = 0;
                foreach (var langcode in Globals.Settings.Languages)
                {
                    var obj = Instantiate(pref.gameObject, pref.parent);
                    obj.transform.localPosition = Vector3.right * 60f * i;
                    obj.GetComponentInChildren<TextMeshPro>().text = langcode.ToUpperInvariant();
                    obj.GetComponent<EventListener>().OnMouseClick.AddListener(()=>ChangeLangTo(langcode));
                    i++;
                }
                Destroy(pref.gameObject);
                LangOptions.transform.Translate(Vector3.left*60f*(i-1)/2f); //center the container
            }
            else
            {
                LangOptions.SetActive(false);    
            }
        }
    
        // You can call this from your own button in the scene, just match the lang shortcode from the settings
        public void ChangeLangTo(string langcode)
        {
            Globals.Lang = langcode;
            Debug.Log("Switching lang to: "+langcode);
            StartCoroutine(Globals.Subtitles.ReloadLangBundles());
        }


        protected override void StartScene() // Called after Master scene is loaded, so that Globals class is properly set up
        {
            PlayButton.sprite = Globals.Persistence.HasSavedGame(ProfileName1) ? ContinueIcon : PlayIcon;
        }

        public void PlayPressed()
        {
            if (Globals.Persistence.HasSavedGame(ProfileName1))
            {
                ContinueGame(ProfileName1);
            }
            else
            {
                StartNewGame(ProfileName1);
            }
        }

        private void StartNewGame(string profileName)
        {
            Globals.Persistence.ResetAll();
            Globals.Persistence.SetProfile(profileName);
            Globals.GameManager.LoadScene(StartingScene);
        }

        private void ContinueGame(string profileName)
        {
            var dest = Globals.Persistence.LoadProfile(profileName);
            if (dest == null)
            {
                Debug.LogError("Loading profile failed.");
                return;
            }

            Globals.GameManager.LoadScene(dest.LastScene);
        }

        public void ResetProfile()
        {
            Globals.Persistence.DeleteProfile(ProfileName1);
            PlayButton.sprite = PlayIcon;
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}