using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class Lexer<T> where T : struct
    {
        private readonly List<ITokenMatcher<T>> _matchers;

        protected Lexer(IEnumerable<ITokenMatcher<T>>? matchers)
        {
            _matchers = matchers?.ToList() ?? new List<ITokenMatcher<T>>();
        }

        private Union<bool, Token<T>> GetNextToken(ReadOnlyMemory<char> source)
        {
            if (source.Length == 0) return new Union<bool, Token<T>>(true);

            foreach (var matcher in _matchers)
            {
                (int consumed, Token<T> token) = matcher.Match(source);
                if (consumed > 0)
                {
                    return new Union<bool, Token<T>>(token);
                }
            }

            return new Union<bool, Token<T>>(false);
        }

        public IEnumerable<Union<Error, Token<T>>> GetTokens(ReadOnlyMemory<char> source)
        {
            int consumed = 0;
            var remaining = source;
            bool final = false;
            bool matched = true;
            while (matched)
            {
                var result = GetNextToken(remaining);
                if (result.TryPick0(out final, out var token))
                {
                    matched = false;
                }
                else
                {
                    consumed += token.Source.Length;
                    remaining = remaining.Slice(token.Source.Length);
                    yield return new Union<Error, Token<T>>(token);
                }
            }

            if (!final)
            {
                yield return new Union<Error, Token<T>>(new Error($"No patterns match text starting at position {consumed}"));
            }
        }

        public IEnumerable<Token<T>> GetTokensOnly(ReadOnlyMemory<char> source)
        {
            foreach (var lexerResult in GetTokens(source))
            {
                if (lexerResult.TryPick0(out var error, out var token))
                {
                    throw new InvalidDataException(error.Message);
                }
                else
                {
                    yield return token;
                }
            }
        }
    }
}
