namespace DTOMaker.Gentime
{
    public enum ExprToken
    {
        // whitespace tokens
        Non = 0x00,
        EOL = 0x01,
        Spc = 0x02,

        // code tokens
        Null = 0x10,
        Bool = 0x11,
        Num = 0x12,
        Str = 0x13,
        Chr = 0x14,
        Var = 0x15,

        // symbol tokens
        // !
        Bang = 0x21,
        // "
        Quote = 0x22,
        // #
        Hash = 0x23,
        // %
        Percent = 0x25,
        // &
        Amp = 0x26,
        // '
        Tick = 0x27,
        // (
        LParen = 0x28,
        // )
        RParen = 0x29,
        // *
        Star = 0x2A,
        // +
        Plus = 0x2B,
        // ,
        Comma = 0x2C,
        // -
        Dash = 0x2D,
        // .
        Dot = 0x2E,
        // /
        Slash = 0x2F,
        // :
        Colon = 0x3A,
        // ;
        Semi = 0x3B,
        // <
        LAngle = 0x3C,
        // =
        Equals = 0x3D,
        // >
        RAngle = 0x3E,
        // ?
        Quest = 0x3F,
        // @
        At = 0x40,
        // [
        LBrack = 0x5B,
        // \
        Slosh = 0x5C,
        // ]
        RBrack = 0x5D,
        // ^
        Hat = 0x5E,
        // _
        Under = 0x5F,
        // `
        Grave = 0x60,
        // [
        LBrace = 0x7B,
        // |
        Pipe = 0x7C,
        // }
        RBrace = 0x7D,
        // ~
        Tilde = 0x7E,

        // longer symbols
        // ==
        EQU,
        // !=
        NEQ,
        // >=
        GEQ,
        // <=
        LEQ,
        // :=
        Assign,
        // **
        Power,
        // &&
        AND,
        // ||
        OR,
    }
}
