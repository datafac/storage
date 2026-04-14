using DataFac.Memory;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DataFac.Compression.Tests;

internal static class TestHelpers
{
    public static string ToDisplayString(this ReadOnlyMemory<byte> buffer)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;
        foreach (var b in buffer.Span)
        {
            if (index != 0)
            {
                result.Append('-');
            }
            result.Append(b.ToString("X2"));
            index = (index + 1) % 32;
            if (index == 0)
            {
                result.AppendLine();
            }
        }
        return result.ToString();
    }

    public static ReadOnlyMemory<byte> FromDisplayString(this string display)
    {
        var builder = new ReadOnlySequenceBuilder<byte>();
        using var sr = new StringReader(display);
        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            byte[] bytes = line.Split('-').Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
            builder = builder.Append(bytes);
        }
        return builder.Build().Compact();
    }

    /// <summary>
    /// Converts the specified string to a UTF-8 encoded <see cref="System.ReadOnlyMemory{T}"/> of bytes, with
    /// each line separated by a null byte.
    /// </summary>
    /// <remarks>Each line in the input string is encoded as UTF-8 and terminated with a null byte in the
    /// resulting sequence. This is done to avoid the variation in line endings on different platforms.</remarks>
    /// <returns>A <see cref="System.ReadOnlyMemory{T}"/> of bytes containing the UTF-8 encoding of each line.</returns>
    public static ReadOnlyMemory<byte> ToMemory(this string text)
    {
        var builder = new ReadOnlySequenceBuilder<byte>();
        using var sr = new StringReader(text);
        string? line;
        while ((line = sr.ReadLine()) is not null)
        {
            builder = builder.Append(Encoding.UTF8.GetBytes(line));
            builder = builder.Append(new byte[] { 0 });
        }
        return builder.Build().Compact();
    }

}
