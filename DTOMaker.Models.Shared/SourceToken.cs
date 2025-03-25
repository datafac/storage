using System;

namespace DTOMaker.Gentime
{
    public readonly struct SourceToken
    {
        public readonly ReadOnlyMemory<char> Line;
        public readonly string Message;
        public readonly int Number;
        public readonly int Offset;
        public readonly int Length;
        public readonly char Modifier;
        public readonly bool IsUnsigned;
        public readonly TokenKind Kind;

        private SourceToken(TokenKind kind, int number, ReadOnlyMemory<char> line, int offset, int length, bool isUnsigned, char modifier, string message)
        {
            Kind = kind;
            Line = line;
            Number = number;
            Offset = offset;
            Length = length;
            IsUnsigned = isUnsigned;
            Modifier = modifier;
            Message = message;
        }

        public SourceToken(TokenKind kind, int number, ReadOnlyMemory<char> line, int offset, int length, string message = "")
        {
            Kind = kind;
            Line = line;
            Number = number;
            Offset = offset;
            Length = length;
            Message = message;
            IsUnsigned = false;
            Modifier = default;
        }

#if NET8_0_OR_GREATER
        public string StringValue => new string(Line.Span.Slice(Offset, Length));
#else
        public string StringValue => new string(Line.Span.Slice(Offset, Length).ToArray());
#endif

        public SourceToken Extend() => new SourceToken(Kind, Number, Line, Offset, Length + 1, IsUnsigned, Modifier, Message);
        public SourceToken Unsigned() => new SourceToken(Kind, Number, Line, Offset, Length, true, Modifier, Message);
        public SourceToken WithModifier(char modifer) => new SourceToken(Kind, Number, Line, Offset, Length, IsUnsigned, modifer, Message);

        public override string ToString()
        {
            return Kind switch
            {
                TokenKind.Whitespace => " ",
                TokenKind.Error => $"Error:{Message}",
                _ => StringValue,
            };
        }
    }
}
