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
        BlobIdV1 id = orig.GetBlobId();
        id.IsEmbedded.ShouldBeTrue();
        id.ToString().ShouldBe("U:0:");
    }

    [Fact]
    public void BlobHash02NonEmpty()
    {
        var orig = new ReadOnlySequence<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = orig.GetBlobId();
        id.IsEmbedded.ShouldBeFalse();
        id.ToString().ShouldBe("V1.0:256:0:0:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");
    }
}
