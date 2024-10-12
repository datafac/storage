using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public sealed class StringMatcher<TEnum> : ITokenMatcher<TEnum> where TEnum : struct
    {
        private readonly int _keylen;
        private readonly ImmutableDictionary<string, TEnum> _map;
        public StringMatcher(int keylen, IEnumerable<KeyValuePair<string, TEnum>> map)
        {
            _keylen = keylen >= 2 ? keylen : throw new ArgumentOutOfRangeException(nameof(keylen), keylen, null);

            var builder = ImmutableDictionary<string, TEnum>.Empty.ToBuilder();
            foreach (var item in map)
            {
                if (item.Key.Length != keylen)
                    throw new ArgumentException($"Key \"{item.Key}\".Length != {keylen}", nameof(map));
                builder[item.Key] = item.Value;
            }
            _map = builder.ToImmutable();
        }

        public (int, Token<TEnum>) Match(ReadOnlyMemory<char> source)
        {
            if (source.Length < _keylen) return (0, default);
#if NET6_0_OR_GREATER
            string key = new string(source.Span.Slice(0, _keylen));
#else
            string key = new string(source.Span.Slice(0, _keylen).ToArray());
#endif
            return _map.TryGetValue(key, out TEnum kind)
                ? (_keylen, new Token<TEnum>(kind, source.Slice(0, _keylen)))
                : (0, default);
        }
    }
}
