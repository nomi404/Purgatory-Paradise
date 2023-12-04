using UnityEngine;

namespace CharlesEngine
{
    public class QuitGameMenuButton : MonoBehaviour
    {
        public SceneField Destination;
        public void QuitGame()
        {
            Globals.Pause.Resume();
            Globals.GameManager.LoadScene(Destination);
        }
    }
}