using UnityEngine;

namespace CharlesEngine
{
    public class ForkNodeFollowLastAnswer : CustomForkStrategy
    {
        public override void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree, OnChoiceHandler choiceCallback)
        {
            int numAnswers = 0;
            foreach (var forkOption in options)
            {
                if (forkOption.Condition) numAnswers++;
            }

            if (numAnswers > 1)
            {
                ForkNodeClassic.Instance.Run(forkNode, options, dialogDialogTree, choiceCallback);
                return;
            }

            if (numAnswers == 0)
            {
                Debug.LogError("No answers available");
                return;
            }
            
            // exactly one answers -> follow immediately as if the user clicked it.
            foreach (var forkOption in options)
            {
                if (forkOption.Condition)
                {
                    choiceCallback.Invoke(forkOption.Node);                        
                }
            }
        }
    }
}