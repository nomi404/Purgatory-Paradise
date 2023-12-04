using System.Collections;

namespace CharlesEngine
{
    public class PauseDialogRoutine : CERoutine
    {
        private bool _shouldContinue;
        public override IEnumerator RunRoutine()
        {
            _shouldContinue = false;
            while (!_shouldContinue)
            {
                yield return null;
            } 
        }

        public void ContinueDialog()
        {
            _shouldContinue = true;
        }
    }
}