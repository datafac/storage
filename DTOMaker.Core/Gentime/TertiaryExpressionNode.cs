namespace DTOMaker.Gentime
{
    public partial class TertiaryExpressionNode : Node
    {
        public static TertiaryExpressionNode Create(TertiaryOperator op, Node n1, Node n2, Node n3) => new TertiaryExpressionNode() { Op = op, Node1 = n1, Node2 = n2, Node3 = n3 };
        public override string ToString()
        {
            return Op switch
            {
                TertiaryOperator.IfThenElse => $"({Node1} ? {Node2} : {Node3})",
                _ => $"({Op}({Node1},{Node2},{Node3})"
            };
        }
    }
}
