namespace DTOMaker.Gentime
{
    public readonly struct ParserResult<TNode>
    {
        public readonly bool Matched;
        public readonly int Consumed;
        public readonly TNode Result;

        public ParserResult(int consumed, TNode result)
        {
            Matched = true;
            Consumed = consumed;
            Result = result;
        }
    }
}
