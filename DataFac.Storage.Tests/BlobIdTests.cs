using System;
using Xunit;
using FluentAssertions;
using System.Linq;
using Inventory.Store;

namespace Inventory.Tests;

public class BlobIdTests
{
    [Fact]
    public void BlobId01Default()
    {
        BlobId id = default;
        id.IsEmpty.Should().BeTrue();
        id.ToString().Should().Be("V1/");
    }

    [Fact]
    public void BlobId02CreateNew()
    {
        BlobId id = new();
        id.IsEmpty.Should().BeTrue();
        id.ToString().Should().Be("V1/");
    }

    [Fact]
    public void BlobId03CreateWithData()
    {
        BlobId id = new BlobId(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        id.IsEmpty.Should().BeFalse();
        id.Equals(default).Should().BeFalse();
        id.ToString().Should().Be("V1/000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F");
    }

    [Fact]
    public void BlobId04CreateFails()
    {
        var ex = Assert.ThrowsAny<ArgumentException>(() =>
        {
            BlobId id = new BlobId(Enumerable.Range(0, 10).Select(i => (byte)i).ToArray());
        });
        ex.Message.Should().StartWith("Expected id.Length to be 32");
    }

    [Fact]
    public void BlobId05CopyEmpty()
    {
        BlobId orig = default;
        BlobId copy = new BlobId(orig);
        orig.IsEmpty.Should().BeTrue();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

    [Fact]
    public void BlobId06CopyNonEmpty()
    {
        BlobId orig = new BlobId(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        BlobId copy = new BlobId(orig);
        copy.IsEmpty.Should().BeFalse();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

    [Fact]
    public void BlobId07UnsafeWrap()
    {
        BlobId orig = new BlobId(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        BlobId copy = BlobId.UnsafeWrap(orig.Id);
        copy.IsEmpty.Should().BeFalse();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

}
