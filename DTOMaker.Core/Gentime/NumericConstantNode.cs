using System;

namespace DTOMaker.Gentime
{
    public partial class NumericConstantNode
    {
        public static NumericConstantNode Create(long value) => new IntegerConstantNode() { Value = value };
        public static NumericConstantNode Create(double value) => new DoubleConstantNode() { Value = value };
        public static NumericConstantNode Create(ReadOnlyMemory<char> source)
        {
            string sourceStr = new string(source.ToArray());
            if (long.TryParse(sourceStr, out var longValue))
            {
                return new IntegerConstantNode() { Value = longValue };
            }
            else
                return new DoubleConstantNode() { Value = double.Parse(sourceStr) };
        }
    }
}
