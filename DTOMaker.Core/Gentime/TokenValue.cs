using System;

namespace DTOMaker.Gentime
{
    public sealed class TokenValue
    {
        //todo? remove this type
        public Int32 Index { get; }
        public String? StringValue { get; }
        public Int64 Int64Value { get; }
        public Boolean BooleanValue { get; }
        public Double DoubleValue { get; }

        public TokenValue() => Index = 0;
        public TokenValue(string value)
        {
            Index = 1; StringValue = value;
        }
        public TokenValue(long value)
        {
            Index = 2; Int64Value = value;
        }
        public TokenValue(bool value)
        {
            Index = 3; BooleanValue = value;
        }
        public TokenValue(double value)
        {
            Index = 4; DoubleValue = value;
        }

        //public static TokenValue Create(object? value)
        //{
        //    return value switch
        //    {
        //        null => new TokenValue(),
        //        string s => new TokenValue(s),
        //        int iValue => new TokenValue(iValue),
        //        long lValue => new TokenValue(lValue ),
        //        bool bValue => new TokenValue(bValue),
        //        double dValue => new TokenValue(dValue),
        //        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        //    };
        //}

        public object? Value
        {
            get
            {
                return Index switch
                {
                    0 => null,
                    1 => StringValue,
                    2 => Int64Value,
                    3 => BooleanValue,
                    4 => DoubleValue,
                    _ => $"InvalidIndex({Index})",
                };
            }
        }
    }
}
