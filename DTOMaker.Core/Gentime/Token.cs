using System;

namespace DTOMaker.Gentime
{
    public readonly struct Token<TEnum> where TEnum : struct
    {
        public readonly TEnum Kind;
        public readonly ReadOnlyMemory<char> Source;

        public Token(TEnum kind, ReadOnlyMemory<char> source) : this()
        {
            Kind = kind;
            Source = source;
        }
    }
}
