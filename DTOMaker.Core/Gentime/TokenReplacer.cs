using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    internal class TokenReplacer
    {
        private readonly ILanguage _language;
        public ImmutableDictionary<string, object?> Tokens;

        public TokenReplacer(ILanguage options, ImmutableDictionary<string, object?> tokens)
        {
            _language = options;
            Tokens = tokens;
        }

        public TokenReplacer Clone(ImmutableDictionary<string, object?> extraTokens)
        {
            return new TokenReplacer(_language, Tokens.AddRange(extraTokens));
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
                    string search = _language.TokenPrefix + item.Key + _language.TokenSuffix;
                    string replace = _language.GetValueAsCode(item.Value);
                    result = result.Replace(search, replace);
                }
            } while (result != lastResult);
            return result;
        }
    }
}
