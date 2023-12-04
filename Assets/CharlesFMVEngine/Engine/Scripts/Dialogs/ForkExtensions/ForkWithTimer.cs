using System.Collections;
using UnityEngine;

namespace CharlesEngine
{
    public class ForkWithTimer : CustomForkStrategy
    {
        public Timer Timer;
        private Node _defaultNode;
        private OnChoiceHandler _callback;
        public override void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree,
            OnChoiceHandler choiceCallback)
        {
            Timer.gameObject.SetActive(true);
            Timer.Run();
            Timer.OnTimer.AddListener(OnTimeOut);
            ForkNodeClassic.Instance.Run(forkNode, options, dialogDialogTree, OnUserChoice);

            // Now we pick the last option, taking conditions into account
            var filteredOptions = ForkNodeClassic.NodesToArray(options);
            var lastOption = filteredOptions[filteredOptions.Length - 1];
            _defaultNode = lastOption;
            _callback = choiceCallback;
        }

        private void OnTimeOut()
        {
            StopTimer();
            StartCoroutine(ShowDefaultAnswer());
        }

        private IEnumerator ShowDefaultAnswer()
        {
            Globals.Choices.HideAllOptionsButThis(_defaultNode);
            yield return new WaitForSeconds(1.5f);
            _callback.Invoke(_defaultNode);
        }

        private void OnUserChoice(Node node)
        {
            StopTimer();
            _callback.Invoke(node);
        }
        
        private void StopTimer()
        {
            Timer.Cancel();
            Timer.OnTimer.RemoveListener(OnTimeOut);
            Timer.gameObject.SetActive(false);
        }
    }
}