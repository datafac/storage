using Shouldly;
using System.IO;
using System.Linq;
using Xunit;

namespace DTOMaker.Gentime.Tests
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
    }
}