using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace DynaText
{
    public static class DynaTextExtensions
    {
        private static readonly Dictionary<char, string> _map = new Dictionary<char, string>()
        {
            // char             code
            [LexChar.Percent] = EscCode.Percent,
            [LexChar.SemiColon] = EscCode.SemiColon,
            [LexChar.DblQuote] = EscCode.DblQuote,
            [LexChar.BackSlash] = EscCode.BackSlash,
            [LexChar.EqualCh] = EscCode.EqualCh,
            [LexChar.Comma] = EscCode.Comma,
            [LexChar.LeftParen] = EscCode.LeftParen,
            [LexChar.RightParen] = EscCode.RightParen,
            [LexChar.LeftCurly] = EscCode.LeftCurly,
            [LexChar.RightCurly] = EscCode.RightCurly,
            [LexChar.LeftSquare] = EscCode.LeftSquare,
            [LexChar.RightSquare] = EscCode.RightSquare,
            [LexChar.LeftAngle] = EscCode.LeftAngle,
            [LexChar.RightAngle] = EscCode.RightAngle,
        };
        private static ImmutableDictionary<char, string> BuildCharToCodeMap()
        {
            return ImmutableDictionary<char, string>.Empty.AddRange(_map
                .Where(kvp => kvp.Value.Length == 2));
        }
        private static ImmutableDictionary<string, char> BuildCodeToCharMap()
        {
            return ImmutableDictionary<string, char>.Empty.AddRange(_map
                .Where(kvp => kvp.Value.Length == 2)
                .Select(kvp => new KeyValuePair<string, char>(kvp.Value, kvp.Key)));
        }
        private static readonly ImmutableDictionary<char, string> escapeCharToCode = BuildCharToCodeMap();
        private static readonly ImmutableDictionary<string, char> escapeCodeToChar = BuildCodeToCharMap();
        /// <summary>
        /// Escapes special chars as %xx;
        /// </summary>
        public static string Escaped(this string value)
        {
            ReadOnlySpan<char> span = value.AsSpan();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < span.Length; i++)
            {
                char ch = span[i];
                if (escapeCharToCode.TryGetValue(ch, out string? code))
                {
                    result.Append($"%{code};");
                }
                else
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }
        public static string UnEscape(this string value)
        {
            ReadOnlySpan<char> span = value.AsSpan();

            // shortcuts
            if (span.IndexOf(LexChar.Percent) < 0) return value;

            StringBuilder result = new StringBuilder();
            int pos = 0;
            while (pos < span.Length)
            {
                char ch = span[pos];
                if (ch == LexChar.Percent)
                {
                    if (pos + 3 < span.Length && span[pos + 3] == LexChar.SemiColon)
                    {
                        // ok
                        char ch1 = span[pos + 1];
                        char ch2 = span[pos + 2];
                        string code = new string([ch1, ch2]);
                        if (escapeCodeToChar.TryGetValue(code, out char decoded))
                        {
                            result.Append(decoded);
                        }
                        else
                        {
                            // invalid escape code - just emit as is
                            result.Append(LexChar.Percent);
                            result.Append(code);
                            result.Append(LexChar.SemiColon);
                        }
                        // next
                        pos = pos + 4;
                    }
                    else
                    {
                        // bad format - just emit as is
                        result.Append(ch);
                        //next
                        pos++;
                    }
                }
                else
                {
                    result.Append(ch);
                    //next
                    pos++;
                }
            }
            return result.ToString();
        }
        public static IEnumerable<SourceToken> ReadAllTokens(this TextReader reader)
        {
            string? text;
            int line = 0;
            while ((text = reader.ReadLine()) is not null)
            {
                var sourceLine = new SourceLine(line, text.AsMemory());
                foreach (var token in sourceLine.ReadLineTokens())
                {
                    yield return token;
                }
                line++;
            }
        }

        private static IEnumerable<SourceToken> ReadLineTokens(this SourceLine source)
        {
            SourceToken token = default;
            int offset = -1;
            for (int i = 0; i < source.Line.Length; i++) 
            {
                char ch = source.Line.Span[i];
                offset++;

                bool consumed = false;
                while (!consumed)
                {
                    consumed = true;
                    if (token.Kind == TokenKind.None)
                    {
                        if (char.IsWhiteSpace(ch))
                        {
                            // start of whitespace
                            token = new SourceToken(TokenKind.Whitespace, source.Number, source.Line, offset, 1);
                        }
                        else if (char.IsDigit(ch))
                        {
                            // start of number
                            token = new SourceToken(TokenKind.Number, source.Number, source.Line, offset, 1);
                        }
                        else if (char.IsLetter(ch) || ch == '_')
                        {
                            // start of identifier
                            token = new SourceToken(TokenKind.Identifier, source.Number, source.Line, offset, 1);
                        }
                        else if (ch == LexChar.DblQuote)
                        {
                            // start of string
                            token = new SourceToken(TokenKind.String, source.Number, source.Line, offset + 1, 0);
                        }
                        else
                        {
                            yield return ch switch
                            {
                                LexChar.LeftCurly => new SourceToken(TokenKind.LeftCurly, source.Number, source.Line, offset, 1),
                                LexChar.RightCurly => new SourceToken(TokenKind.RightCurly, source.Number, source.Line, offset, 1),
                                LexChar.Comma => new SourceToken(TokenKind.Comma, source.Number, source.Line, offset, 1),
                                LexChar.LeftSquare => new SourceToken(TokenKind.LeftSquare, source.Number, source.Line, offset, 1),
                                LexChar.RightSquare => new SourceToken(TokenKind.RightSquare, source.Number, source.Line, offset, 1),
                                LexChar.EqualCh => new SourceToken(TokenKind.Equals, source.Number, source.Line, offset, 1),
                                _ => new SourceToken(TokenKind.Error, source.Number, source.Line, offset, 1, "Unexpected character"),
                            };
                        }
                    }
                    else if (token.Kind == TokenKind.Whitespace)
                    {
                        // try consume whitespace
                        if (char.IsWhiteSpace(ch))
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.Number)
                    {
                        // try consume number
                        if (char.IsDigit(ch) || ch == '.')
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.Identifier)
                    {
                        // try consume identifier
                        if (char.IsLetterOrDigit(ch) || ch == '_')
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.String)
                    {
                        // try consume string
                        if (ch == LexChar.DblQuote)
                        {
                            // end of string
                            yield return token;
                            token = default;
                        }
                        else
                        {
                            token = token.Extend();
                        }
                    }
                    else
                    {
                        yield return new SourceToken(TokenKind.Error, source.Number, source.Line, offset, 1, "Unexpected state");
                        token = default;
                    }
                } // while !consumed

            } // foreach ch

            // emit remaining token
            if (token.Kind == TokenKind.String)
            {
                // non-terminated string!
                yield return new SourceToken(TokenKind.Error, source.Number, source.Line, offset, 1, "Unterminated string");
            }
            else if (token.Kind != TokenKind.None)
            {
                yield return token;
            }
        }

        public static ParseResult ParseTokens(this ReadOnlySpan<SourceToken> tokens, TokenKind exitToken = TokenKind.None)
        {
            int curlyCount = 0;
            int squareCount = 0;
            int otherCount = 0;
            int offset = 0;

            if (tokens.Length == 0 && exitToken != TokenKind.None) return new ParseResult(0, "Unexpected EOF.");

            bool exitSeen = false;
            var token = tokens[0];
            while (offset < tokens.Length)
            {
                token = tokens[offset++];

                if (token.Kind == exitToken)
                {
                    exitSeen = true;
                    break;
                }

                ParseResult result = default;
                switch (token.Kind)
                {
                    case TokenKind.Error: throw new InvalidDataException(token.Message);
                    case TokenKind.LeftCurly:
                        result = tokens.Slice(offset).ParseTokens(TokenKind.RightCurly);
                        if (result.IsError) return result;
                        offset += result.Consumed;
                        break;
                    case TokenKind.RightCurly:
                        curlyCount--;
                        break;
                    case TokenKind.LeftSquare:
                        result = tokens.Slice(offset).ParseTokens(TokenKind.RightSquare);
                        if (result.IsError) return result;
                        offset += result.Consumed;
                        break;
                    case TokenKind.RightSquare:
                        squareCount--;
                        break;
                    default:
                        otherCount++;
                        break;
                }
            }

            if (curlyCount != 0) return new ParseResult(offset, $"Unbalanced {{}} found at (L{token.Number},C{token.Offset}).");

            if (squareCount != 0) return new ParseResult(offset, $"Unbalanced [] found at (L{token.Number},C{token.Offset}).");

            return (exitSeen || exitToken == TokenKind.None)
                ? new ParseResult(offset)
                : new ParseResult(0, "Unexpected EOF.");
        }

    }
}
