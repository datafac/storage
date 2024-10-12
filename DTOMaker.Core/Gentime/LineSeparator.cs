using System;

namespace DTOMaker.Gentime
{
    public sealed class LineSeparator<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _kind;

        public LineSeparator(TEnum kind)
        {
            _kind = kind;
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length <= 0 || source.Span[0] != '\r' && source.Span[0] != '\n') return (0, default);
            int count = 1;
            while (source.Length > count && (source.Span[count] == '\r' || source.Span[count] == '\n'))
            {
                count++;
            }
            return (count, new Token<TEnum>(_kind, source.Slice(0, count)));
        }
    }
}
