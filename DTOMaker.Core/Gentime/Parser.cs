using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class Parser<TEnum, TNode> where TEnum : struct
    {
        private readonly Lexer<TEnum> _lexer;

        protected Parser(Lexer<TEnum> lexer)
        {
            _lexer = lexer;
        }

        protected abstract bool OnTryMatch(ReadOnlyMemory<Token<TEnum>> tokens, out int consumed, out TNode? result);
        protected abstract IEnumerable<Token<TEnum>> OnFilterTokens(IEnumerable<Token<TEnum>> tokens);
        protected abstract TNode OnMakeErrorNode(string message);

        public TNode Parse(ReadOnlyMemory<char> source)
        {
            var rawTokens = new List<Token<TEnum>>();
            foreach (var lexerResult in _lexer.GetTokens(source))
            {
                if (lexerResult.TryPick0(out var error, out var token))
                {
                    return OnMakeErrorNode(error.Message);
                }
                else
                {
                    rawTokens.Add(token);
                }
            }

            Token<TEnum>[] tokens = OnFilterTokens(rawTokens).ToArray();

            if (OnTryMatch(tokens, out int consumed, out var result))
            {
                if (consumed != tokens.Length)
                {
                    return OnMakeErrorNode($"Not all source matched. Only {consumed} of {tokens.Length} tokens consumed.");
                }
                else
                {
                    return result!;
                }
            }
            else
            {
                return OnMakeErrorNode($"Parse unsuccessfull. Only {consumed} of {tokens.Length} tokens consumed.");
            }
        }
    }
}
