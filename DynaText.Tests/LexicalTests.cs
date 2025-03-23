using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;

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

        [Theory]
        [InlineData(0, """ """)]
        [InlineData(1, """{}""")]
        [InlineData(2, """{A=1,B=2}""")]
        [InlineData(3, """{A=[],B=[1],C=[1,2]}""")]
        [InlineData(4, """{A={X=1,Y=2}}""")]
        public void Parse0_Success_InputIsValid(int _, string input)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
        }

        [Theory]
        [InlineData(0, """{""", "Unexpected EOF.")]
        [InlineData(1, """A=1,B=2}""", "Unexpected token: Identifier(A) found at (L0,C0).")]
        [InlineData(2, """{A=[],B=[1,C=[1,2]}""", "Unexpected identifier: 'C' found at (L0,C11).")]
        [InlineData(3, """{A={X=1,Y=2}""", "Unexpected EOF.")]
        [InlineData(4, """{[}]""", "Unexpected token: LeftSquare([) found at (L0,C1).")]
        public void Parse1_Failures_UnbalancedInput(int _, string input, string expectedError)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBe(expectedError);
        }

        [Fact]
        public async Task Roundtrip0_Empty()
        {
            DynaTextMap orig = new DynaTextMap();

            using var writer = new StringWriter();
            bool emitted = orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaTextMap? copy = result.Output as DynaTextMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaTextMap>();
            copy.ShouldBe(orig);
        }

    }
}