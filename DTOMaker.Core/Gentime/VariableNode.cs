namespace DTOMaker.Gentime
{
    public partial class VariableNode
    {
        public static VariableNode Create(string name) => new VariableNode() { Name = name };
        public override string ToString() => Name ?? "_no_name_";
    }
}
