using System;
using System.Collections.Generic;

namespace CharlesEngine
{
    public class ForkNodeWithSafety : CustomForkStrategy
    {
        public string FallbackNodeId;
        public override void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree, OnChoiceHandler choiceCallback)
        {
            int numAnswers = 0;
            foreach (var forkOption in options)
            {
                if (forkOption.Condition) numAnswers++;
            }

            if (numAnswers >= 1)
            {
                ForkNodeClassic.Instance.Run(forkNode, options, dialogDialogTree, choiceCallback);
                return;
            }

            var node = dialogDialogTree.GetNodeFromName(FallbackNodeId);
            if (node == null)
            {
                throw new Exception("No node found:"+FallbackNodeId);
            }

            var tempNode = new Node(999, node.Rect, NodeType.ChoiceAnswer);
            tempNode.ReadableID = "tempNode";
            tempNode.Connections = new List<int> {node.ID};
            choiceCallback.Invoke(tempNode);
        }
    }
}