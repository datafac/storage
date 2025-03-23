namespace DynaText
{
    public readonly struct ParseResult
    {
        public readonly int Consumed;
        public readonly string? Message;
        public ParseResult(int consumed, string? message = null)
        {
            Consumed = consumed;
            Message = message;
        }

        public bool IsError => Message is not null;
    }
}
