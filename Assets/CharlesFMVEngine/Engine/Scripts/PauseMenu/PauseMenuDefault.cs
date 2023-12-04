namespace CharlesEngine
{
    public class PauseMenuDefault : PauseMenuBase
    {
        public override void Init()
        {
            GetComponent<FadeTween>().InitAlpha();
            GetComponent<FadeTween>().Run();
            GetComponentInChildren<QuitPauseMenuButton>().OnMouseClick.AddListener(ResumePauseClicked);
        }

        private void ResumePauseClicked()
        {
            OnPauseCancel.Invoke();
        }
    }
}