using TMPro;
using UnityEngine;

namespace CharlesEngine
{
    public class SubtitlesButton : MonoBehaviour
    {
        public SpriteRenderer SubsButton;
        public Sprite SubsOnSprite;
        public Sprite SubsOffSprite;

        private void OnEnable()
        {
            SubsButton.sprite = Globals.Subtitles.UserEnabled ? SubsOnSprite : SubsOffSprite;
        }
        
        public void ToggleSubtitles()
        {
            Globals.Subtitles.ToggleEnabled();
            SubsButton.sprite = Globals.Subtitles.UserEnabled ? SubsOnSprite : SubsOffSprite;
        }
    }
}