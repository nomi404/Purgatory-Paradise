namespace CharlesEngine
{
    public class JumpToNode : CEScript
    {
        public DialogManager DialogManager;

        public Dialogues Dialog;
        public string Tree;
        public int TreeIndex;
        public string Node;


        public override void Run()
        {
            DialogManager.JumpTo(Tree, Node);
        }

        private void Reset()
        {
            DialogManager = FindObjectOfType<DialogManager>();
            Dialog = DialogManager?.Dialogues;
        }
    }
}