using System;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace DataFac.Storage.Tests;

public class BlobIdTests
{
    [Fact]
    public void BlobId01CreateDefault()
    {
        BlobIdV1 id = default;
        id.IsEmpty.Should().BeTrue();
        id.ToString().Should().Be("");
    }

    [Fact]
    public void BlobId02CreateNew()
    {
        BlobIdV1 id = new();
        id.IsEmpty.Should().BeTrue();
        id.ToString().Should().Be("");
    }

    [Fact]
    public void BlobId03CreateFromSpanWithZeroData()
    {
        ReadOnlySpan<byte> input = stackalloc byte[64];
        BlobIdV1 id = new BlobIdV1(input);
        id.IsEmpty.Should().BeTrue();
        id.Equals(default).Should().BeTrue();
        id.ToString().Should().Be("");
    }

    [Fact]
    public void BlobId04CreateFromSpanWithInvalidData()
    {
        Span<byte> input = stackalloc byte[64];
        Enumerable.Range(0, 64).Select(i => (byte)i).ToArray().CopyTo(input);
        BlobIdV1 id = new BlobIdV1(input);
        id.IsEmpty.Should().BeFalse();
        id.ToString().Should().Be("V2.3:185207048:4:252579084:5:ICEiIyQlJicoKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj8=");
    }

    [Fact]
    public void BlobId05CreateFromSpanWithValidData()
    {
        string inputStr =
            "00-00-01-00-00-01-00-00-00-01-00-00-00-00-00-00-" +
            "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-" +
            "40-AF-F2-E9-D2-D8-92-2E-47-AF-D4-64-8E-69-67-49-" +
            "71-58-78-5F-BD-1D-A8-70-E7-11-02-66-BF-94-48-80";

        Span<byte> input = stackalloc byte[64];
        inputStr.Split('-').Select(s => (byte)int.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray().CopyTo(input);
        BlobIdV1 id = new BlobIdV1(input);
        id.IsEmpty.Should().BeFalse();
        id.ToString().Should().Be("V1.0:256:0:0:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");

        string.Join("-", id.ToArray().Select(b => b.ToString("X2"))).Should().Be(inputStr);
    }

    [Fact]
    public void BlobId06CreateFromCopy()
    {
        BlobData data = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 orig = data.GetId();
        BlobIdV1 copy = new BlobIdV1(orig);
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
        copy.GetHashCode().Should().Be(orig.GetHashCode());
    }

    [Fact]
    public void BlobId07CreateFails()
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() =>
        {
            ReadOnlySpan<byte> input = stackalloc byte[10];
            BlobIdV1 id = new BlobIdV1(input);
        });
        ex.Message.Should().StartWith("Expected length to be 64.");
    }

}
