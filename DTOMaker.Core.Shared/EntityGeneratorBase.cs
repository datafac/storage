using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DTOMaker.Gentime
{
    public abstract class EntityGeneratorBase
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly TokenStack _tokenStack = new TokenStack();
        private readonly ILanguage _language;

        protected EntityGeneratorBase(ILanguage language)
        {
            _language = language;
        }

        private string ReplaceTokens(string input)
        {
            // note token recursion not supported
            var tokenPrefix = _language.TokenPrefix.AsSpan();
            var tokenSuffix = _language.TokenSuffix.AsSpan();

            ReadOnlySpan<char> inputSpan = input.AsSpan();

            // fast exit for lines with no tokens
            if (inputSpan.IndexOf(tokenPrefix) < 0) return input;

            StringBuilder result = new StringBuilder();
            bool replaced = false;
            int remainderPos = 0;
            do
            {
                ReadOnlySpan<char> remainder = inputSpan.Slice(remainderPos);
                int tokenPos = remainder.IndexOf(tokenPrefix);
                int tokenEnd = tokenPos < 0 ? -1 : remainder.Slice(tokenPos + tokenPrefix.Length).IndexOf(tokenSuffix);
                if (tokenPos >= 0 && tokenEnd >= 0)
                {
                    // token found!
                    var tokenSpan = remainder.Slice(tokenPos + tokenPrefix.Length, tokenEnd);
                    string tokenName = tokenSpan.ToString();
                    if (_tokenStack.Top.TryGetValue(tokenName, out var tokenValue))
                    {
                        // replace valid token
                        // - emit prefix
                        // - emit token
                        // - calc remainder
                        ReadOnlySpan<char> prefix = remainder.Slice(0, tokenPos);
                        result.Append(prefix.ToString());
                        result.Append(_language.GetValueAsCode(tokenValue));
                        remainderPos += (tokenPos + tokenPrefix.Length + tokenEnd + tokenSuffix.Length);
                        replaced = true;
                    }
                    else
                    {
                        // invalid token - emit error then original line
                        result.Clear();
                        result.AppendLine($"#error The token '{_language.TokenPrefix}{tokenName}{_language.TokenSuffix}' on the following line is invalid.");
                        result.AppendLine(input);
                        return result.ToString();
                    }
                }
                else
                {
                    // no token - emit remainder and return
                    result.Append(remainder.ToString());
                    return result.ToString();
                }
            }
            while (replaced);

            return result.ToString();
        }

        protected void Emit(string line)
        {
            _builder.AppendLine(ReplaceTokens(line));
        }

        protected IDisposable NewScope(ModelScopeBase scope)
        {
            return _tokenStack.NewScope(scope.Tokens);
        }

        protected abstract void OnGenerate(ModelScopeEntity scope);
        public string GenerateSourceText(ModelScopeEntity scope)
        {
            using var _ = NewScope(scope);
            _builder.Clear();
            OnGenerate(scope);
            return _builder.ToString();
        }
    }
}
