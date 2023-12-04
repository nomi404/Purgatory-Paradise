using UnityEngine;

namespace CharlesEngine
{
    public class ReplaceDialogLayout : CEScript
    {
        public GameObject DialogLayoutPrefab;
        public override void Run()
        {
            Globals.Choices.ReplaceLayout(DialogLayoutPrefab);
        }
    }
}