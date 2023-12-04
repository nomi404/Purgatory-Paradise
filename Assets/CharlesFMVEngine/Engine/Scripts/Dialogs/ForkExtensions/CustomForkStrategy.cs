using UnityEngine;

namespace CharlesEngine
{
    public abstract class CustomForkStrategy : MonoBehaviour, IForkNode
    {
        public Dialogues Dialog;
        public string Tree;
        public string ForkId;

        private void Start()
        {
            foreach (var t in Dialog.Trees)
            {
                if (t.Name == Tree)
                {
                    var node = t.GetNodeFromName(ForkId);
                    if (node == null)
                    {
                        break;
                    }
                    node.ForkType = this;
                    return;
                }
            }
            Debug.LogError("Fork not found:"+Tree+"/"+ForkId);
        }

        public abstract void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree, OnChoiceHandler choiceCallback);
    }
}