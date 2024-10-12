using System;

namespace DTOMaker.Gentime
{
    public sealed class SingleQuotedChar<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _kind;

        public SingleQuotedChar(TEnum kind)
        {
            _kind = kind;
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length >= 4 && source.Span[0] == '\'' && source.Span[1] == '\\' && source.Span[3] == '\'')
                return (4, new Token<TEnum>(_kind, source.Slice(0, 4)));
            if (source.Length >= 3 && source.Span[0] == '\'' && source.Span[2] == '\'')
                return (3, new Token<TEnum>(_kind, source.Slice(0, 3)));
            return default;
        }
    }
}
