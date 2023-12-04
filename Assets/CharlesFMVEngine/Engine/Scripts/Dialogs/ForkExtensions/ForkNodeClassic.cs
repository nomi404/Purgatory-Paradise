using System.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace CharlesEngine
{
    public class ForkNodeClassic : IForkNode
    {

        public static ForkNodeClassic Instance = new ForkNodeClassic();
        
        public virtual void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree, OnChoiceHandler choiceCallback)
        {
            if (forkNode.Video != null)
            {
                Globals.Videos.LoopVideo(forkNode.Video);
            }
            else
            {
                Debug.LogWarning("No fork video!");
            }
            var optionsArray = NodesToArray(options);
            if (optionsArray.Length == 0)
            {
                Debug.LogWarning("No dialog choices shown in forknode:"+forkNode.ReadableID);
            }
            Globals.Choices.ShowChoices( optionsArray );
            Globals.Choices.OnChoiceSelected = choiceCallback;
        }

        public static Node[] NodesToArray(ForkOption[] options)
        {
            var result = new List<Node>();
            for (int i = 0; i < options.Length; i++)
            {
                if (!options[i].Condition)
                {
                    continue;
                }
                result.Add(options[i].Node);
            }
            return result.ToArray();
        }
    }
}