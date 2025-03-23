using System;

namespace DynaText
{
    public readonly struct SourceLine
    {
        private readonly static SourceLine _empty = new SourceLine(0, ReadOnlyMemory<char>.Empty);
        public static SourceLine Empty => _empty;

        public readonly int Number;
        public readonly ReadOnlyMemory<char> Line;

        public SourceLine(int number, ReadOnlyMemory<char> line)
        {
            Number = number;
            Line = line;
        }
    }
}
