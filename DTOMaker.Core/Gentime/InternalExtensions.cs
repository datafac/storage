using System;

namespace DTOMaker.Gentime
{
    internal static class InternalExtensions
    {
        public static string ToCamelCase(this string value)
        {
            ReadOnlySpan<char> input = value.AsSpan();
            Span<char> output = stackalloc char[input.Length];
            input.CopyTo(output);
            for (int i = 0; i < output.Length; i++)
            {
                if (Char.IsLetter(output[i]))
                {
                    output[i] = Char.ToLower(output[i]);
                    return new string(output.ToArray());
                }
            }
            return new string(output.ToArray());
        }

        public static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> memory)
        {
            var span = memory.Span;
            int index = 0;
            while (index < span.Length && Char.IsWhiteSpace(span[index]))
            {
                index++;
            }
            return memory.Slice(index);
        }
        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> memory)
        {
            var span = memory.Span;
            int length = span.Length;
            while (length > 0 && Char.IsWhiteSpace(span[length - 1]))
            {
                length--;
            }
            return memory.Slice(0, length);
        }
        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> memory)
        {
            return memory.TrimStart().TrimEnd();
        }
        private static bool SequencesAreEqual(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
        {
            if (other.Length != span.Length) return false;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != other[i]) return false;
            }
            return true;
        }
        public static bool SequenceEqual(this Span<char> span, string other) => InternalExtensions.SequencesAreEqual(span, other.AsSpan());
    }
}
