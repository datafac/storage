using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    internal class TokenReplacer
    {
        private readonly ILanguage _options;
        public ImmutableDictionary<string, object?> Tokens;

        public TokenReplacer(ILanguage options, ImmutableDictionary<string, object?> tokens)
        {
            _options = options;
            Tokens = tokens;
        }

        public TokenReplacer Clone(ImmutableDictionary<string, object?> extraTokens)
        {
            return new TokenReplacer(_options, Tokens.AddRange(extraTokens));
        }

        public string ReplaceTokens(string input)
        {
            string result = input;
            string lastResult;
            do
            {
                lastResult = result;
                foreach (var item in Tokens)
                {
                    string search = _options.TokenPrefix + item.Key + _options.TokenSuffix;
                    string replace = item.Value is null ? "" : (item.Value?.ToString() ?? "");
                    result = result.Replace(search, replace);
                }
            } while (result != lastResult);
            return result;
        }
    }
}
