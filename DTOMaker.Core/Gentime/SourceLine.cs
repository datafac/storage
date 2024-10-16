namespace DTOMaker.Gentime
{
    public readonly struct SourceLine
    {
        private readonly static SourceLine _empty = new SourceLine(0, string.Empty);
        public static SourceLine Empty => _empty;

        public readonly int Line;
        public readonly string Text;

        public SourceLine(int line, string text)
        {
            Line = line;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
