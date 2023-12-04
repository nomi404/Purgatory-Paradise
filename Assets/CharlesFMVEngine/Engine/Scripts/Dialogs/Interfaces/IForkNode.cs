namespace CharlesEngine
{
    public delegate void OnChoiceHandler(Node node);
    public interface IForkNode
    {
        
        void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree,OnChoiceHandler choiceCallback);
    }
}