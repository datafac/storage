using DataFac.Memory;
using Shouldly;
using Snappier;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

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

public class CompressionTests
{
    private static readonly string sourcePara1 =
        """
        Twas bryllyg, and þe slythy toves
        Did gyre and gymble in þe wabe:
        All mimsy were þe borogoves;
        And þe mome raths outgrabe.
        """;
    private static Octets octetsPara1 = new Octets(Encoding.UTF8.GetBytes(sourcePara1));

    private static readonly string sourcePara2 =
        """
        Don't think me unkind
        Words are hard to find
        They're only cheques I've left unsigned
        From the banks of chaos in my mind
        And when their eloquence escapes me
        Their logic ties me up and rapes me
        Do-do-do-do, do-da-da-da
        Is all I want to say to you
        Do-do-do-do, do-da-da-da
        Their innocence will pull me through
        """;
    private static Octets octetsPara2 = new Octets(Encoding.UTF8.GetBytes(sourcePara2));

    private static Octets originalData = Octets.Combine(octetsPara1, octetsPara2);

    [Theory]
    [InlineData(BlobCompAlgo.UnComp, 0, (byte)'U')]
    [InlineData(BlobCompAlgo.Brotli, 1, (byte)'B')]
    [InlineData(BlobCompAlgo.Snappy, 2, (byte)'S')]
    public void RoundtripBlobCompAlgos(BlobCompAlgo algo, byte expectedByteValue, byte expectedCharCode)
    {
        byte byteValue = (byte)algo;
        byteValue.ShouldBe(expectedByteValue);

        byte charCode = algo.ToCharCode();
        charCode.ShouldBe(expectedCharCode);
        charCode.ToCompAlgo().ShouldBe(algo);
    }

    [Fact]
    public void RoundtripViaSnappier()
    {
        var compressionBuffers = new ByteBufferWriter();
        Snappy.Compress(originalData.ToSequence(), compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();

        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);

        var decompressed = Octets.Wrap(decompressionBuffers.GetWrittenSequence());
        decompressed.Equals(originalData).ShouldBeTrue();
    }
}

public class SnappierRegressionTests
{
    [Fact]
    public async Task SnappierRegression01Empty()
    {
        var data = ReadOnlyMemory<byte>.Empty;
        Span<byte> hashSpan = stackalloc byte[32];
        var compressResult = SnappyCompressor.CompressData(data, hashSpan);
        // check compressed
        compressResult.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        compressResult.HashAlgo.ShouldBe(BlobHashAlgo.None);
        compressResult.Output.ToArray().ShouldBeEquivalentTo(data.ToArray());
        // check regression
        string display = compressResult.Output.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression02OneChar()
    {
        var data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(new string('a', 1)));
        Span<byte> hashSpan = stackalloc byte[32];
        var compressResult = SnappyCompressor.CompressData(data, hashSpan);
        // check compressed
        compressResult.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        compressResult.HashAlgo.ShouldBe(BlobHashAlgo.None);
        compressResult.Output.ToArray().ShouldBeEquivalentTo(data.ToArray());
        var compressed = compressResult.Output;
        // check regression
        string display = compressResult.Output.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression03ShortString()
    {
        var text = new string('a', 10);
        var data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(text));
        Span<byte> hashSpan = stackalloc byte[32];
        var compressResult = SnappyCompressor.CompressData(data, hashSpan);
        // check compressed
        compressResult.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        compressResult.HashAlgo.ShouldBe(BlobHashAlgo.None);
        compressResult.Output.ToArray().ShouldBeEquivalentTo(data.ToArray());
        // check regression
        string display = compressResult.Output.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression04LongString()
    {
        var text = new string('a', 64);
        var data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(text));
        Span<byte> hashSpan = stackalloc byte[32];
        var compressResult = SnappyCompressor.CompressData(data, hashSpan);
        // check compressed
        compressResult.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        compressResult.HashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressResult.Output);
        copy.ToArray().ShouldBeEquivalentTo(data.ToArray());
        // check regression
        string display = compressResult.Output.ToDisplayString();
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
        // note: we convert multi-line text to bytes, normalizing line endings.
        var data = originalText.ToMemory();
        Span<byte> hashSpan = stackalloc byte[32];
        var compressResult = SnappyCompressor.CompressData(data, hashSpan);
        // check compressed
        compressResult.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        compressResult.HashAlgo.ShouldBe(BlobHashAlgo.Sha256);
        // check decompressed
        var copy = SnappyCompressor.Decompress(compressResult.Output);
        copy.ToArray().ShouldBeEquivalentTo(data.ToArray());
        // check regression
        string display = compressResult.Output.ToDisplayString();
        await Verifier.Verify(display);
    }

    [Fact]
    public async Task SnappierRegression06DecompressNet48AndNet80ShouldBeSame()
    {
        var data = originalText.ToMemory();

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
        net48Copy.ToArray().ShouldBeEquivalentTo(data.ToArray());

        var net80Copy = SnappyCompressor.Decompress(net80Compressed.FromDisplayString());
        net80Copy.ToArray().ShouldBeEquivalentTo(data.ToArray());
    }
}
