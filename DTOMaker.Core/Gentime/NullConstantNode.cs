using System;

namespace DTOMaker.Gentime
{
    public partial class NullConstantNode
    {
        private static readonly NullConstantNode _null = new NullConstantNode();
        public static NullConstantNode Create(ReadOnlyMemory<char> source) => _null;
        public override string ToString() => "null";
    }
}
