using System;
using System.Collections.Generic;
using System.IO;

namespace DynaText
{
    public readonly struct ParseResult
    {
        public readonly int Consumed;
        public readonly object? Output;
        public readonly string? Message;
        public ParseResult(string message)
        {
            Consumed = 0;
            Message = message;
            Output = null;
        }
        public ParseResult(int consumed, object? output)
        {
            Consumed = consumed;
            Output = output;
            Message = null;
        }

        public bool IsError => Message is not null;
    }
}