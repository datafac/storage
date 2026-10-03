using Shouldly;
using System;
using Xunit;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace DataFac.Storage.Tests;

public class BlobKeyTests
{
    [Fact]
    public void BlobKey01Equals()
    {
        var bytes1 = new byte[] { 1, 2, 3, 4 };
        var bytes2 = new byte[] { 1, 2, 3, 4 };
        var bytes3 = new byte[] { 5, 6, 7, 8 };
        var key1 = BlobKey.From(bytes1);
        var key2 = BlobKey.From(bytes2);
        var key3 = BlobKey.From(bytes3);
        var key4 = BlobKey.NotFound();
        key1.Equals(key2).ShouldBeTrue();
        key1.Equals(key3).ShouldBeFalse();
        key1.Equals(key4).ShouldBeFalse();
        key4.Equals(key4).ShouldBeTrue();
    }

    [Fact]
    public void BlobKey02GetHashCode()
    {
        var bytes1 = new byte[] { 1, 2, 3, 4 };
        var bytes2 = new byte[] { 1, 2, 3, 4 };
        var bytes3 = new byte[] { 5, 6, 7, 8 };
        var key1 = BlobKey.From(bytes1);
        var key2 = BlobKey.From(bytes2);
        var key3 = BlobKey.From(bytes3);
        key1.GetHashCode().ShouldBe(key2.GetHashCode());
        key1.GetHashCode().ShouldNotBe(key3.GetHashCode());
    }

    [Theory]
    [InlineData(new byte[] { 1 }, "AQ==")]
    [InlineData(new byte[] { 1, 2 }, "AQI=")]
    [InlineData(new byte[] { 1, 2, 3 }, "AQID")]
    [InlineData(new byte[] { 1, 2, 3, 4 }, "AQIDBA==")]
    [InlineData(new byte[] { 255 }, ".w==")]
    [InlineData(new byte[] { 255, 255 }, "..8=")]
    [InlineData(new byte[] { 255, 255, 255 }, "....")]
    public void BlobKey03ToString(byte[] input, string expected)
    {
        var key = BlobKey.From(input);
        string actual = key.ToString();
        actual.ShouldBe(expected);
    }

    [Fact]
    public void BlobKey04TryFormat()
    {
        var bytes1 = new byte[] { 1, 2, 3, 4 };
        string expected = Convert.ToBase64String(bytes1);

        var key1 = BlobKey.From(bytes1);
        Span<char> buffer = stackalloc char[16];
        bool result = key1.TryFormat(buffer, out int charsWritten, default, null);
        result.ShouldBeTrue();
        charsWritten.ShouldBe(expected.Length);
#if NET8_0_OR_GREATER
        string actual = new string(buffer.Slice(0, charsWritten));
#else
        string actual = new string(buffer.Slice(0, charsWritten).ToArray());
#endif
        actual.ShouldBe(expected);
    }
}
