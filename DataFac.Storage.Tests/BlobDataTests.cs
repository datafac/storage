using Xunit;
using FluentAssertions;
using System.Linq;
using Inventory.Store;

namespace Inventory.Tests;

public class BlobDataTests
{
    [Fact]
    public void BlobData01Default()
    {
        BlobData data = default;
        data.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void BlobData02CreateNew()
    {
        BlobData data = new();
        data.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void BlobData03CreateWithData()
    {
        BlobData data = new BlobData(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        data.IsEmpty.Should().BeFalse();
        data.Equals(default).Should().BeFalse();
    }

    [Fact]
    public void BlobData05CopyEmpty()
    {
        BlobData orig = default;
        BlobData copy = new BlobData(orig);
        orig.IsEmpty.Should().BeTrue();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

    [Fact]
    public void BlobData06CopyNonEmpty()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        orig.Memory.Length.Should().Be(256);
        BlobData copy = new BlobData(orig);
        copy.IsEmpty.Should().BeFalse();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

    [Fact]
    public void BlobData07UnsafeWrap()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        orig.Memory.Length.Should().Be(256);
        BlobData copy = BlobData.UnsafeWrap(orig.Memory);
        copy.IsEmpty.Should().BeFalse();
        copy.Should().Be(orig);
        copy.Equals(orig).Should().BeTrue();
    }

}
