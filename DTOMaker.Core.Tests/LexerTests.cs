using FluentAssertions;
using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime.Tests
{
    public class LexerTests
    {
        [Theory]
        [InlineData(ExprToken.Bang, '!', 0x21)]
        [InlineData(ExprToken.Quote, '"', 0x22)]
        [InlineData(ExprToken.Hash, '#', 0x23)]
        [InlineData(ExprToken.Percent, '%', 0x25)]
        [InlineData(ExprToken.Amp, '&', 0x26)]
        [InlineData(ExprToken.Tick, '\'', 0x27)]
        [InlineData(ExprToken.LParen, '(', 0x28)]
        [InlineData(ExprToken.RParen, ')', 0x29)]
        [InlineData(ExprToken.Star, '*', 0x2A)]
        [InlineData(ExprToken.Plus, '+', 0x2B)]
        [InlineData(ExprToken.Comma, ',', 0x2C)]
        [InlineData(ExprToken.Dash, '-', 0x2D)]
        [InlineData(ExprToken.Dot, '.', 0x2E)]
        [InlineData(ExprToken.Slash, '/', 0x2F)]
        [InlineData(ExprToken.Colon, ':', 0x3A)]
        [InlineData(ExprToken.Semi, ';', 0x3B)]
        [InlineData(ExprToken.LAngle, '<', 0x3C)]
        [InlineData(ExprToken.Equals, '=', 0x3D)]
        [InlineData(ExprToken.RAngle, '>', 0x3E)]
        [InlineData(ExprToken.Quest, '?', 0x3F)]
        [InlineData(ExprToken.At, '@', 0x40)]
        [InlineData(ExprToken.LBrack, '[', 0x5B)]
        [InlineData(ExprToken.Slosh, '\\', 0x5C)]
        [InlineData(ExprToken.RBrack, ']', 0x5D)]
        [InlineData(ExprToken.Hat, '^', 0x5E)]
        [InlineData(ExprToken.Under, '_', 0x5F)]
        [InlineData(ExprToken.Grave, '`', 0x60)]
        [InlineData(ExprToken.LBrace, '{', 0x7B)]
        [InlineData(ExprToken.Pipe, '|', 0x7C)]
        [InlineData(ExprToken.RBrace, '}', 0x7D)]
        [InlineData(ExprToken.Tilde, '~', 0x7E)]
        public void CheckChar(ExprToken kind, char ch, ushort expectedCode)
        {
            ushort ord = ch;
            ord.Should().Be(expectedCode, $"hex value of '{ch}' is 0x{ord:X2}");
            ((ushort)kind).Should().Be(expectedCode);
        }

        [Fact]
        public void Lex01_Whitespace()
        {
            var source = """   """; // 3 spaces
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            tokens.Count.Should().Be(1);
            tokens[0].Kind.Should().Be(ExprToken.Spc);
            tokens[0].Source.Length.Should().Be(3);
            string.Join(" ", tokens.ToDisplayStrings()).Should().Be("Spc[   ]");
        }

        [Fact]
        public void Lex02_WholeNumber()
        {
            var source = """1234567890""";
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            tokens.Count.Should().Be(1);
            tokens[0].Kind.Should().Be(ExprToken.Num);
            tokens[0].Source.Length.Should().Be(10);
            string.Join(" ", tokens.ToDisplayStrings()).Should().Be("1234567890");
        }

        [Fact]
        public void Lex03_QuotedString()
        {
            var source =
                """
                "a string"
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            tokens.Count.Should().Be(1);
            tokens[0].Kind.Should().Be(ExprToken.Str);
            tokens[0].Source.Length.Should().Be(10);
            string.Join(" ", tokens.ToDisplayStrings()).Should().Be("\"a string\"");
        }

        [Fact]
        public void Lex04_Multiple()
        {
            var source =
                """
                123456789
                "a string"
                _firstName
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            string.Join(" ", tokens.SelectCodeTokens().ToDisplayStrings()).Should().Be("123456789 \"a string\" [_firstName]");
        }

        [Fact]
        public void Lex05_ExpressionWithWhitespace()
        {
            var source =
                """
                y := ( a * a + b * b ) ** ( 1 / 2 )
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            string.Join(" ", tokens.SelectCodeTokens().ToDisplayStrings()).Should()
                .Be("[y] := ( [a] * [a] + [b] * [b] ) ** ( 1 / 2 )");
        }

        [Fact]
        public void Lex06_ExpressionSansWhitespace()
        {
            var source =
                """
                y:=(a*a+b*b)**(1/2)
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            string.Join(" ", tokens.SelectCodeTokens().ToDisplayStrings()).Should()
                .Be("[y] := ( [a] * [a] + [b] * [b] ) ** ( 1 / 2 )");
        }

        [Fact]
        public void Lex06_QuotedChars()
        {
            var source =
                """
                'x' '\r'
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            string.Join(" ", tokens.ToDisplayStrings()).Should().Be("'x' Spc[ ] '\\r'");
        }

        [Fact]
        public void Lex07_Keywords()
        {
            var source =
                """
                null false true
                """;
            var lexer = new ExprLexer();

            // act
            var errors = new List<Error>();
            var tokens = new List<Token<ExprToken>>();
            foreach (var result in lexer.GetTokens(source.AsMemory())) { result.Switch(errors.Add, tokens.Add); }

            // assert
            errors.Should().BeEmpty();
            string.Join(" ", tokens.SelectCodeTokens().ToDisplayStrings()).Should().Be("null false true");
        }

    }
}
