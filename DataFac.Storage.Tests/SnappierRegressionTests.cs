using DataFac.Memory;
using Shouldly;
using Snappier;
using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DataFac.Storage.Tests;

public class SnappierRegressionTests
{
    [Fact]
    public async Task SnappierRegression01Empty()
    {
        var orig = ReadOnlySequence<byte>.Empty;
        var compressed = SnappyCompressor.Compress(orig);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressed);
        copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
        // check regression
        string display = compressed.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression02OneChar()
    {
        var orig = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 1)));
        var compressed = SnappyCompressor.Compress(orig);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressed);
        copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
        // check regression
        string display = compressed.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression03ShortString()
    {
        var orig = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 10)));
        var compressed = SnappyCompressor.Compress(orig);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressed);
        copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
        // check regression
        string display = compressed.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression04LongString()
    {
        var orig = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(new string('a', 64)));
        var compressed = SnappyCompressor.Compress(orig);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressed);
        copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
        // check regression
        string display = compressed.ToDisplayString();
        await Verifier.Verify(display);
    }

    private static readonly string originalText =
            """
            The rain in Spain falls mainly on the plain.
            Please explain my pain and disdain or I will go insain [sic].
            Plain Jain is a brain in a train in Spain.
            Maine is the main domain to obtain the brain drain.";
            """;

    [Fact]
#if NET8_0_OR_GREATER
    public async Task SnappierRegression05MultiLineText_Net80()
#else
    public async Task SnappierRegression05MultiLineText_Net48()
#endif
    {
        var orig = originalText.ToByteSequence();
        var compressed = SnappyCompressor.Compress(orig);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressed);
        copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
        // check regression
        string display = compressed.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression06DecompressNet48AndNet80ShouldBeSame()
    {
        var orig = originalText.ToByteSequence();

        var net48Compressed =
            """
            CC-01-34-54-68-65-20-72-61-69-6E-20-69-6E-20-53-70-01-09-90-66-61-6C-6C-73-20-6D-61-69-6E-6C-79
            20-6F-6E-20-74-68-65-20-70-6C-61-69-6E-2E-00-50-6C-65-61-73-65-20-65-78-70-01-10-0C-20-6D-79-20
            05-32-6C-61-6E-64-20-64-69-73-64-61-69-6E-20-6F-72-20-49-20-77-69-6C-6C-20-67-6F-20-69-6E-73-01
            14-1C-5B-73-69-63-5D-2E-00-50-05-35-00-4A-01-12-18-69-73-20-61-20-62-72-05-0B-10-6E-20-61-20-74
            11-0B-05-83-18-2E-00-4D-61-69-6E-65-01-26-01-7D-01-8B-08-20-64-6F-05-07-14-74-6F-20-6F-62-74-01
            38-01-1A-38-62-72-61-69-6E-20-64-72-61-69-6E-2E-22-3B-00
            """;

        var net80Compressed =
            """
            CC-01-34-54-68-65-20-72-61-69-6E-20-69-6E-20-53-70-01-09-90-66-61-6C-6C-73-20-6D-61-69-6E-6C-79
            20-6F-6E-20-74-68-65-20-70-6C-61-69-6E-2E-00-50-6C-65-61-73-65-20-65-78-70-01-10-0C-20-6D-79-20
            05-32-1C-61-6E-64-20-64-69-73-64-01-3E-3C-6F-72-20-49-20-77-69-6C-6C-20-67-6F-20-69-6E-73-01-14
            10-5B-73-69-63-5D-01-3E-01-0D-00-4A-01-05-14-69-73-20-61-20-62-11-78-08-61-20-74-11-0B-05-83-18
            2E-00-4D-61-69-6E-65-01-26-01-7D-01-8B-08-20-64-6F-05-07-14-74-6F-20-6F-62-74-01-43-01-1A-38-62
            72-61-69-6E-20-64-72-61-69-6E-2E-22-3B-00
            """;

        var net48Copy = SnappyCompressor.Decompress(net48Compressed.FromDisplayString());
        net48Copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());

        var net80Copy = SnappyCompressor.Decompress(net80Compressed.FromDisplayString());
        net80Copy.ToArray().ShouldBeEquivalentTo(orig.ToArray());
    }
}
