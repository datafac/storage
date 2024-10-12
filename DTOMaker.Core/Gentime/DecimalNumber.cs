using System;

namespace DTOMaker.Gentime
{
    public sealed class DecimalNumber<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _kind;

        public DecimalNumber(TEnum kind)
        {
            _kind = kind;
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            // todo exponents
            if (source.Length <= 0) return default;

            // numbers start with a digit or sign
            int count;
            if (source.Length >= 1 && char.IsDigit(source.Span[0]))
            {
                count = 1;
            }
            else if (source.Length >= 2 && (source.Span[0] == '-' || source.Span[0] == '+') && char.IsDigit(source.Span[1]))
            {
                count = 2;
            }
            else
            {
                return default;
            }

            // parse integer part - one or more digits or '_' or  one '.'
            bool seenDot = false;
            while (source.Length > count && !seenDot && (source.Span[count] == '.' || source.Span[count] == '_' || char.IsDigit(source.Span[count])))
            {
                seenDot = source.Span[count] == '.';
                count++;
            }
            if (!seenDot) return (count, new Token<TEnum>(_kind, source.Slice(0, count)));
            // parse fractional part - one or more digits or '_'
            while (source.Length > count && (source.Span[count] == '_' || char.IsDigit(source.Span[count])))
            {
                count++;
            }
            return (count, new Token<TEnum>(_kind, source.Slice(0, count)));
        }
    }
}
