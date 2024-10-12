namespace DTOMaker.Gentime
{
    public class SymbolConstantNode : ConstantNode
    {
        public static SymbolConstantNode Create(Token<ExprToken> value)
        {
            var node = new SymbolConstantNode() { Value = value };
            node.Freeze();
            return node;
        }

        public Token<ExprToken> Value { get; set; }
        public override string ToString() => Value.ToDisplayString();
    }
}
