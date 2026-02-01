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
        Snappy.Compress(originalData.Sequence, compressionBuffers);
        var compressed = compressionBuffers.GetWrittenSequence();

        var decompressionBuffers = new ByteBufferWriter();
        Snappy.Decompress(compressed, decompressionBuffers);

        var decompressed = Octets.UnsafeWrap(decompressionBuffers.GetWrittenSequence());
        decompressed.Equals(originalData).ShouldBeTrue();
    }
}
