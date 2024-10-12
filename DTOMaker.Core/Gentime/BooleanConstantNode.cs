using System;

namespace DTOMaker.Gentime
{
    public partial class BooleanConstantNode : ConstantNode
    {
        private static readonly BooleanConstantNode _true = new BooleanConstantNode() { Value = true };
        private static readonly BooleanConstantNode _false = new BooleanConstantNode() { Value = false };
        public static BooleanConstantNode Create(ReadOnlyMemory<char> source) => bool.Parse(new string(source.ToArray())) ? _true : _false;
        public override string ToString() => Value ? "true" : "false";
    }
}
