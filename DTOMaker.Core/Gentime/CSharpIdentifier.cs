using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public sealed class CSharpIdentifier<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly TEnum _defaultKind;
        private readonly ImmutableDictionary<string, TEnum> _keywords;

        public CSharpIdentifier(TEnum defaultKind, IEnumerable<KeyValuePair<string, TEnum>> keywords)
        {
            _defaultKind = defaultKind;
            _keywords = ImmutableDictionary<string, TEnum>.Empty.AddRange(keywords);
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length <= 0) return default;

            // identifiers start with a letter or '_'
            if (source.Span[0] != '_' && !char.IsLetter(source.Span[0])) return default;

            // identifiers continue with letters, digits or '_'
            int count = 1;
            while (source.Length > count && (source.Span[count] == '_' || char.IsLetterOrDigit(source.Span[count])))
            {
                count++;
            }
            // special case identifiers: true, false, null
#if NET6_0_OR_GREATER
            string keyword = new string(source.Span.Slice(0, count));
#else
            string keyword = new string(source.Span.Slice(0, count).ToArray());
#endif
            if (_keywords.TryGetValue(keyword, out TEnum kind))
            {
                return (count, new Token<TEnum>(kind, source.Slice(0, count)));
            }
            else
                return (count, new Token<TEnum>(_defaultKind, source.Slice(0, count)));
        }
    }
}
