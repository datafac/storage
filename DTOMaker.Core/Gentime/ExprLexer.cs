using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class ExprLexer : Lexer<ExprToken>
    {
        private static readonly List<ITokenMatcher<ExprToken>> _matchers =
        [
            // order is important here
            new LineSeparator<ExprToken>(ExprToken.EOL),
            new WhiteSpace<ExprToken>(ExprToken.Spc),
            new DecimalNumber<ExprToken>(ExprToken.Num),
            new CSharpIdentifier<ExprToken>(ExprToken.Var, new Dictionary<string, ExprToken>()
            {
                ["true"] = ExprToken.Bool,
                ["false"] = ExprToken.Bool,
                ["null"] = ExprToken.Null,
            }),
            new DoubleQuotedString<ExprToken>(ExprToken.Str),
            new SingleQuotedChar<ExprToken>(ExprToken.Chr),
            // double char symbol matchers
            new StringMatcher<ExprToken>(2,
                new Dictionary<string, ExprToken>()
                {
                    //["<<"] = MyToken.SHL,
                    //[">>"] = MyToken.SHR,
                    ["=="] = ExprToken.EQU,
                    ["!="] = ExprToken.NEQ,
                    [">="] = ExprToken.GEQ,
                    ["<="] = ExprToken.LEQ,
                    [":="] = ExprToken.Assign,
                    ["**"] = ExprToken.Power,
                    ["&&"] = ExprToken.AND,
                    ["||"] = ExprToken.OR,
                    // +=
                    // -=
                    // *=
                    // /=
                    // %=
                }),
            // single char symbol matchers
            new CharMatcher<ExprToken>(
                new Dictionary<char, ExprToken>()
                {
                    ['='] = ExprToken.Equals,
                    ['+'] = ExprToken.Plus,
                    ['-'] = ExprToken.Dash,
                    ['*'] = ExprToken.Star,
                    ['/'] = ExprToken.Slash,
                    ['%'] = ExprToken.Percent,
                    ['#'] = ExprToken.Hash,
                    ['!'] = ExprToken.Bang,
                    ['^'] = ExprToken.Hat,
                    ['.'] = ExprToken.Dot,
                    ['&'] = ExprToken.Amp,
                    ['@'] = ExprToken.At,
                    ['?'] = ExprToken.Quest,
                    [':'] = ExprToken.Colon,
                    //['"'] = MyToken.Quote, conflicts with literal string
                    //['\''] = MyToken.Tick, conflicts with literal char
                    ['\\'] = ExprToken.Slosh,
                    ['('] = ExprToken.LParen,
                    [')'] = ExprToken.RParen,
                    ['['] = ExprToken.LBrack,
                    [']'] = ExprToken.RBrack,
                    ['{'] = ExprToken.LBrace,
                    ['}'] = ExprToken.RBrace,
                    ['<'] = ExprToken.LAngle,
                    ['>'] = ExprToken.RAngle,
                    ['~'] = ExprToken.Tilde,
                    //['_'] = MyToken.Under, conflicts with CSharpName
                }),
        ];

        public ExprLexer() : base(_matchers) { }
    }
}
