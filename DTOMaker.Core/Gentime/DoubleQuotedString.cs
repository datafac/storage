using System;

namespace DTOMaker.Gentime
{
    public sealed class DoubleQuotedString<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _kind;

        public DoubleQuotedString(TEnum kind)
        {
            _kind = kind;
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length <= 0) return default;
            if (source.Span[0] != '"') return default;

            int count = 1;

            while (source.Length > count)
            {
                char ch = source.Span[count];

                if (ch == '"')
                {
                    // end of string
                    return (count + 1, new Token<TEnum>(_kind, source.Slice(0, count + 1)));
                }
                else if (ch == '\\' && source.Length > count + 1)
                {
                    // escaped char
                    count++;
                }
                else
                {
                    // accumulate
                }

                // next
                count++;
            }

            // not closed!
            return default;
        }
    }
}
