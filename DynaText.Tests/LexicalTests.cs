using Shouldly;
using System;
using System.IO;
using System.Linq;

namespace DynaText.Tests
{
    public class LexicalTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("abc.def", "abc.def")]
        [InlineData("10%", "10%pc;")]
        [InlineData("{abc}", "%lc;abc%rc;")]
        [InlineData("(abc)", "%lp;abc%rp;")]
        [InlineData("[abc]", "%ls;abc%rs;")]
        [InlineData("<abc>", "%la;abc%ra;")]
        [InlineData("abc\\def", "abc%bs;def")]
        [InlineData("abc\"def", "abc%dq;def")]
        public void Lexer0_StringEscaping(string value, string expectedEncoding)
        {
            // encode
            string encoded = value.Escaped();
            encoded.ShouldBe(expectedEncoding);

            // decode
            string decoded = encoded.UnEscape();
            decoded.ShouldBe(value);
        }
        [Theory]
        [InlineData(0, """{""", TokenKind.LeftCurly)]
        [InlineData(1, """}""", TokenKind.RightCurly)]
        [InlineData(2, """ """, TokenKind.Whitespace)]
        [InlineData(3, """abc""", TokenKind.Identifier)]
        [InlineData(4, """123""", TokenKind.Number)]
        [InlineData(5, """=""", TokenKind.Equals)]
        public void Lexer1_Success(int _, string encoded, TokenKind expectedTokenKind)
        {
            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThanOrEqualTo(1);
            tokens[0].Kind.ShouldBe(expectedTokenKind);
        }

        [Fact]
        public void Lexer2_Failure_UnexpectedChar()
        {
            string encoded =
                """
                .
                """;

            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThan(0);
            var token = tokens.Last();
            token.Kind.ShouldBe(TokenKind.Error);
            token.Message.ShouldBe("Unexpected character");
        }

        [Fact]
        public void Lexer3_Failure_UnterminatedString()
        {
            string encoded =
                """
                "abc
                """;

            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThan(0);
            var token = tokens.Last();
            token.Kind.ShouldBe(TokenKind.Error);
            token.Message.ShouldBe("Unterminated string");
        }

        internal readonly struct ScanResult
        {
            public readonly int Consumed;
            public readonly string? Message;
            public ScanResult(int consumed, string? message = null)
            {
                Consumed = consumed;
                Message = message;
            }

            public bool IsError => Message is not null;
        }

        private static ScanResult CheckBalance(ReadOnlySpan<SourceToken> tokens, TokenKind exitToken = TokenKind.None)
        {
            int curlyCount = 0;
            int squareCount = 0;
            int otherCount = 0;
            int offset = 0;

            if (tokens.Length == 0 && exitToken != TokenKind.None) return new ScanResult(0, "Unexpected EOF.");

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

                ScanResult result = default;
                switch (token.Kind)
                {
                    case TokenKind.Error: throw new InvalidDataException(token.Message);
                    case TokenKind.LeftCurly:
                        result = CheckBalance(tokens.Slice(offset), TokenKind.RightCurly);
                        if (result.IsError) return result;
                        offset += result.Consumed;
                        break;
                    case TokenKind.RightCurly:
                        curlyCount--;
                        break;
                    case TokenKind.LeftSquare:
                        result = CheckBalance(tokens.Slice(offset), TokenKind.RightSquare);
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

            if (curlyCount != 0) return new ScanResult(offset, $"Unbalanced {{}} found at (L{token.Number},C{token.Offset}).");

            if (squareCount != 0) return new ScanResult(offset, $"Unbalanced [] found at (L{token.Number},C{token.Offset}).");

            return (exitSeen || exitToken == TokenKind.None)
                ? new ScanResult(offset)
                : new ScanResult(0, "Unexpected EOF.");
        }

        [Theory]
        [InlineData(0, """ """)]
        [InlineData(1, """{}""")]
        [InlineData(2, """{A=1,B=2}""")]
        [InlineData(3, """{A=[],B=[1],C=[1,2]}""")]
        [InlineData(4, """{A={X=1,Y,2}}""")]
        public void Parse0_Success_InputIsValid(int _, string input)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            var result = CheckBalance(tokens);
            result.Message.ShouldBeNull();
        }

        [Theory]
        [InlineData(0, """{""", "Unexpected EOF.")]
        [InlineData(1, """A=1,B=2}""", "Unbalanced {} found at (L0,C7).")]
        [InlineData(2, """{A=[],B=[1,C=[1,2]}""", "Unbalanced {} found at (L0,C18).")]
        [InlineData(3, """{A={X=1,Y=2}""", "Unexpected EOF.")]
        [InlineData(4, """{[}]""", "Unbalanced {} found at (L0,C3).")]
        public void Parse1_Failures_UnbalancedInput(int _, string input, string expectedError)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            var result = CheckBalance(tokens);
            result.Message.ShouldBe(expectedError);
        }

    }
}