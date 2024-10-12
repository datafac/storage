using System;

namespace DTOMaker.Gentime
{
    public sealed class WhiteSpace<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _kind;

        public WhiteSpace(TEnum kind)
        {
            _kind = kind;
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length <= 0 || !char.IsWhiteSpace(source.Span[0])) return (0, default);
            int count = 1;
            while (source.Length > count && char.IsWhiteSpace(source.Span[count]))
            {
                count++;
            }
            return (count, new Token<TEnum>(_kind, source.Slice(0, count)));
        }
    }
}
