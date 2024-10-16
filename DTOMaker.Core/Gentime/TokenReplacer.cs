using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    internal class TokenReplacer
    {
        private readonly ILanguage _options;
        public Dictionary<string, object?> Tokens;

        public TokenReplacer(ILanguage options, IEnumerable<KeyValuePair<string, object?>> tokens)
        {
            _options = options;
            Tokens = new Dictionary<string, object?>();
            foreach (var token in tokens)
            {
                Tokens[token.Key] = token.Value;
            }
        }

        public TokenReplacer Clone(IEnumerable<KeyValuePair<string, object?>> extraTokens)
        {
            return new TokenReplacer(_options, Tokens.Concat(extraTokens));
        }

        public bool TryGetToken(string name, out object? value)
        {
            return Tokens.TryGetValue(name, out value);
        }

        public void SetToken(string name, object? value)
        {
            Tokens[name] = value;
        }

        public void RemoveToken(string name)
        {
            Tokens.Remove(name);
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
