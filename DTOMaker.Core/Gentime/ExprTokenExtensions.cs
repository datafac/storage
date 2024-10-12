using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public static class ExprTokenExtensions
    {
        public static bool IsCodeToken(this Token<ExprToken> token)
        {
            return (int)token.Kind >= 0x10;
        }

        public static string ToDisplayString(this Token<ExprToken> token)
        {
            return token.Kind switch
            {
                ExprToken.Var => $"[{new string(token.Source.Span.ToArray())}]",
                ExprToken.Spc => $"Spc[{new string(token.Source.Span.ToArray())}]",
                _ => new string(token.Source.Span.ToArray())
            };
        }

        public static IEnumerable<string> ToDisplayStrings(this IEnumerable<Token<ExprToken>> tokens)
        {
            foreach (var token in tokens)
            {
                yield return token.ToDisplayString();
            }
        }

        public static IEnumerable<Token<ExprToken>> SelectCodeTokens(this IEnumerable<Token<ExprToken>> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.IsCodeToken())
                    yield return token;
            }
        }
    }
}
