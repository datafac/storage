using System;

namespace DynaText
{
    public readonly struct SourceToken
    {
        public readonly TokenKind Kind;
        public readonly int Number;
        public readonly ReadOnlyMemory<char> Line;
        public readonly int Offset;
        public readonly int Length;
        public readonly string Message;

        public SourceToken(TokenKind kind, int number, ReadOnlyMemory<char> line, int offset, int length, string message = "")
        {
            Kind = kind;
            Line = line;
            Number = number;
            Offset = offset;
            Length = length;
            Message = message;
        }

#if NET8_0_OR_GREATER
        public string StringValue => new string(Line.Span.Slice(Offset, Length));
#else
        public string StringValue => new string(Line.Span.Slice(Offset, Length).ToArray());
#endif

        public SourceToken Extend() => new SourceToken(Kind, Number, Line, Offset, Length + 1);

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
