namespace CharlesEngine
{
    public struct ForkOption
    {
        public Node Node;
        public bool Condition;
        public ForkOption(Node n, bool cond)
        {
            Node = n;
            Condition = cond;
        }
    }
}