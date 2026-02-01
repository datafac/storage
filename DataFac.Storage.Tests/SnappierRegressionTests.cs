using DataFac.Memory;
using Shouldly;
using Snappier;
using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DataFac.Storage.Tests;

public class SnappierRegressionTests
{
    private static string DisplayString(ReadOnlySequence<byte> sequence)
    {
        StringBuilder result = new StringBuilder();
        int index = 0;
        foreach (var segment in sequence)
        {
            foreach (var b in segment.Span)
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
        }
        return result.ToString();
    }

    [Fact]
    public async Task SnappierRegression01Empty()
    {
        var testSequence = ReadOnlySequence<byte>.Empty;
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(testSequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();
        // check decompressed
        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);
        var decompressed = decompressionBuffers.GetWrittenSequence();
        decompressed.ToArray().ShouldBeEquivalentTo(testSequence.ToArray());
        // check regression
        string display = DisplayString(compressed);
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression02OneChar()
    {
        var testSequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 1)));
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(testSequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();
        // check decompressed
        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);
        var decompressed = decompressionBuffers.GetWrittenSequence();
        decompressed.ToArray().ShouldBeEquivalentTo(testSequence.ToArray());
        // check regression
        string display = DisplayString(compressed);
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression03ShortString()
    {
        var testSequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 10)));
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(testSequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();
        // check decompressed
        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);
        var decompressed = decompressionBuffers.GetWrittenSequence();
        decompressed.ToArray().ShouldBeEquivalentTo(testSequence.ToArray());
        // check regression
        string display = DisplayString(compressed);
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression04LongString()
    {
        var testSequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 64)));
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(testSequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();
        // check decompressed
        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);
        var decompressed = decompressionBuffers.GetWrittenSequence();
        decompressed.ToArray().ShouldBeEquivalentTo(testSequence.ToArray());
        // check regression
        string display = DisplayString(compressed);
        await Verifier.Verify(display);
    }

    [Fact]
#if NET8_0_OR_GREATER
    public async Task SnappierRegression05MultiLineText_Net80()
#else
    public async Task SnappierRegression05MultiLineText_Net48()
#endif
    {
        var lines =
            """
            The rain in Spain falls mainly on the plain.
            Please explain my pain and disdain or I will go insain [sic].
            Plain Jain is a brain in a train in Spain.
            Maine is the main domain to obtain the brain drain.
            """;
        var testSequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(lines));
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(testSequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();
        // check decompressed
        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);
        var decompressed = decompressionBuffers.GetWrittenSequence();
        decompressed.ToArray().ShouldBeEquivalentTo(testSequence.ToArray());
        // check regression
        string display = DisplayString(compressed);
        await Verifier.Verify(display);
    }
}