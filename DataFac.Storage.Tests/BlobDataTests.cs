using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace DataFac.Storage.Tests;

public class BlobDataTests
{
    [Fact]
    public void BlobData01Default()
    {
        ReadOnlyMemory<byte> data = default;
        data.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void BlobData02CreateNew()
    {
        ReadOnlyMemory<byte> data = new();
        data.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void BlobData03CreateWithData()
    {
        ReadOnlyMemory<byte> data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        data.IsEmpty.ShouldBeFalse();
        data.Equals(default).ShouldBeFalse();
    }

}
