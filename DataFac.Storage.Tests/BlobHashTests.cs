using Shouldly;
using System;
using System.Buffers;
using System.Linq;
using Xunit;

namespace DataFac.Storage.Tests;

public class BlobHashTests
{
    [Fact]
    public void BlobHash01Empty()
    {
        ReadOnlySequence<byte> orig = default;
        var result = orig.TryCompressBlob();
        result.BlobId.IsEmbedded.ShouldBeTrue();
        result.BlobId.ToString().ShouldBe("U:0:");
        result.BlobId.TryGetEmbeddedBlob(out var copy).ShouldBeTrue();
        copy.ToArray().SequenceEqual(orig.ToArray()).ShouldBeTrue();
    }

    [Fact]
    public void BlobHash02LargeUncompressed()
    {
        var orig = new ReadOnlySequence<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        var result = orig.TryCompressBlob();
        result.BlobId.IsEmbedded.ShouldBeFalse();
        result.BlobId.BlobSize.ShouldBe(256);
        result.BlobId.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        result.BlobId.ToString().ShouldBe("V1.0:256:U:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");
    }

    [Fact]
    public void BlobHash03LargeCompressedEmbedded()
    {
        var orig = new ReadOnlySequence<byte>(Enumerable.Range(0, 256).Select(i => (byte)0).ToArray());
        var result = orig.TryCompressBlob();
        result.BlobId.IsEmbedded.ShouldBeTrue();
        result.BlobId.ToString().ShouldBe("S:16:gAIAAP4BAP4BAP4BAPoBAA==");
        result.BlobId.TryGetEmbeddedBlob(out var copy).ShouldBeTrue();
        copy.ToArray().SequenceEqual(orig.ToArray()).ShouldBeTrue();
    }

    [Fact]
    public void BlobHash04LargeCompressedNotEmbedded()
    {
        var text =
            """
            The rain in Spain falls mainly on the plain.
            Please explain my pain and disdain or I will go insain [sic].
            Plain Jain is a brain in a train in Spain.
            Maine is the main domain to obtain the brain drain.";
            """;
        var orig = text.ToByteSequence();
        var result = orig.TryCompressBlob();
        result.BlobId.IsEmbedded.ShouldBeFalse();
        result.BlobId.BlobSize.ShouldBe(204);
        result.BlobId.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        result.BlobId.ToString().ShouldBe("V1.0:204:S:1:zvnrkv/TwC40uhpghpCfhUIOTe6tnOzyrmUuBT5rrfA=");
    }
}
