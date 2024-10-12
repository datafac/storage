using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public sealed class CharMatcher<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly ImmutableDictionary<char, TEnum> _map;
        public CharMatcher(IEnumerable<KeyValuePair<char, TEnum>> map)
        {
            _map = ImmutableDictionary<char, TEnum>.Empty.AddRange(map);
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length == 0) return (0, default);
            return _map.TryGetValue(source.Span[0], out TEnum kind)
                ? (1, new Token<TEnum>(kind, source.Slice(0, 1)))
                : (0, default);
        }
    }
}
