using System;

namespace DTOMaker.Gentime
{
    public partial class StringConstantNode
    {
        public static StringConstantNode Create(string value) => new StringConstantNode() { Value = value };
        public static StringConstantNode Create(ReadOnlyMemory<char> source) => new StringConstantNode() { Value = new string(source.Slice(1, source.Length - 2).ToArray()) };
    }
}
