using System;

namespace DTOMaker.Gentime
{
    public sealed class TemplateException : Exception
    {
        private static string buildMessage(string message, SourceLine line) => $"{message}: '{line.Text}' (line {line.Line})";

        public TemplateException() { }
        public TemplateException(string message, SourceLine line) : base(buildMessage(message, line)) { }
    }
}
