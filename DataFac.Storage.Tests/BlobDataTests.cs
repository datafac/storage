using Xunit;
using Shouldly;
using System.Linq;
using DataFac.Storage;

namespace DataFac.Storage.Tests;

public class BlobDataTests
{
    [Fact]
    public void BlobData01Default()
    {
        BlobData data = default;
        data.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void BlobData02CreateNew()
    {
        BlobData data = new();
        data.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void BlobData03CreateWithData()
    {
        BlobData data = new BlobData(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        data.IsEmpty.ShouldBeFalse();
        data.Equals(default).ShouldBeFalse();
    }

    [Fact]
    public void BlobData05CopyEmpty()
    {
        BlobData orig = default;
        BlobData copy = new BlobData(orig);
        orig.IsEmpty.ShouldBeTrue();
        copy.ShouldBe(orig);
        copy.Equals(orig).ShouldBeTrue();
    }

    [Fact]
    public void BlobData06CopyNonEmpty()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        orig.Memory.Length.ShouldBe(256);
        BlobData copy = new BlobData(orig);
        copy.IsEmpty.ShouldBeFalse();
        copy.ShouldBe(orig);
        copy.Equals(orig).ShouldBeTrue();
    }

    [Fact]
    public void BlobData07UnsafeWrap()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        orig.Memory.Length.ShouldBe(256);
        BlobData copy = BlobData.UnsafeWrap(orig.Memory);
        copy.IsEmpty.ShouldBeFalse();
        copy.ShouldBe(orig);
        copy.Equals(orig).ShouldBeTrue();
    }

}
