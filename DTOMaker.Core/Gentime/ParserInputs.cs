using System;

namespace DTOMaker.Gentime
{
    public readonly struct ParserInputs<TEnum> where TEnum : struct
    {
        public readonly ReadOnlyMemory<Token<TEnum>> Source;
        public ParserInputs(ReadOnlyMemory<Token<TEnum>> source) => Source = source;
        public ParserInputs<TEnum> Consume(int consumed) => new ParserInputs<TEnum>(Source.Slice(consumed));
    }
}
