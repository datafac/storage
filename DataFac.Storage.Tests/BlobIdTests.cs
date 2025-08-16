using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace DataFac.Storage.Tests;

public class BlobIdTests
{
    [Fact]
    public void BlobId01CreateDefault()
    {
        BlobIdV1 id = default;
        id.IsDefault.ShouldBeTrue();
        id.ToString().ShouldBe("");
    }

    [Fact]
    public void BlobId02CreateWithZeroData()
    {
        ReadOnlySpan<byte> input = stackalloc byte[BlobIdV1.Size];
        BlobIdV1 id = BlobIdV1.FromSpan(input);
        id.IsDefault.ShouldBeTrue();
        id.ToString().ShouldBe("");
        id.Equals(default).ShouldBeFalse();
    }

    [Fact]
    public void BlobId03CreateWithInvalidData()
    {
        ReadOnlyMemory<byte> input = Enumerable.Range(0, BlobIdV1.Size).Select(i => (byte)i).ToArray();
        BlobIdV1 id = BlobIdV1.UnsafeWrap(input);
        id.IsDefault.ShouldBeFalse();
        id.ToString().ShouldBe("V2.3:185207048:4:252579084:5:ICEiIyQlJicoKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj8=");
    }

    [Fact]
    public void BlobId05CreateFromSpanWithValidData()
    {
        string inputStr =
            "00-00-01-00-00-01-00-00-00-01-00-00-00-00-00-00-" +
            "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-" +
            "40-AF-F2-E9-D2-D8-92-2E-47-AF-D4-64-8E-69-67-49-" +
            "71-58-78-5F-BD-1D-A8-70-E7-11-02-66-BF-94-48-80";

        ReadOnlyMemory<byte> input = inputStr.Split('-').Select(s => (byte)int.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray();
        BlobIdV1 id = BlobIdV1.UnsafeWrap(input);
        id.IsDefault.ShouldBeFalse();
        id.ToString().ShouldBe("V1.0:256:0:0:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");

        string.Join("-", id.Memory.ToArray().Select(b => b.ToString("X2"))).ShouldBe(inputStr);
    }

    [Fact]
    public void BlobId06CreateFromCopy()
    {
        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 orig = data.Span.GetBlobId();
        BlobIdV1 copy = orig;
        copy.ShouldBe(orig);
        copy.Equals(orig).ShouldBeTrue();
        copy.GetHashCode().ShouldBe(orig.GetHashCode());
    }

    [Fact]
    public void BlobId07CreateFails()
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() =>
        {
            ReadOnlySpan<byte> input = stackalloc byte[10];
            BlobIdV1 id = BlobIdV1.FromSpan(input);
        });
        ex.Message.ShouldStartWith("Length must be 64.");
    }

}
