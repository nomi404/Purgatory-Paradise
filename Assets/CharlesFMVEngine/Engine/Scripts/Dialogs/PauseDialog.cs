namespace CharlesEngine
{
    public class PauseDialog : CEScript
    {
        public DialogManager DialogManager;
        
        public override void Run()
        {
            DialogManager.Pause();
        }
        
        public void Resume()
        {
            DialogManager.Resume();
        }

        private void Reset()
        {
            DialogManager = FindObjectOfType<DialogManager>(); 
        }
    }
}