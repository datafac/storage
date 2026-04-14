using DataFac.Compression;
using DataFac.Hashing;
using Shouldly;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DataFac.MemBlox2.Tests;

public class EmbeddedIdTests
{
    [Fact]
    public void Embedded01_IdFromEmptyBlob()
    {
        var data = ReadOnlyMemory<byte>.Empty;
        var id = BlobIdV1.FromSpan(data.ToContentId().Span);
        id.IsEmbedded.ShouldBeTrue();
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        id.ToString().ShouldBe("U:0:");
    }

    [Fact]
    public async Task Embedded02_IdFromEmptyText()
    {
        var data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(string.Empty));
        var id = BlobIdV1.FromSpan(data.ToContentId().Span);
        id.IsEmbedded.ShouldBeTrue();
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.ToString().ShouldBe("U:0:");
    }

    [Fact]
    public async Task Embedded03_IdFromNonEmptyBlob()
    {
        var data = new ReadOnlyMemory<byte>(Enumerable.Range(0, 62).Select(i => (byte)i).ToArray());
        var id = BlobIdV1.FromSpan(data.ToContentId().Span);
        id.IsEmbedded.ShouldBeTrue();
        id.CompAlgo.ShouldBe(BlobCompAlgo.UnComp);
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.ToString().ShouldBe("U:62:AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4OTo7PD0=");
    }

    [Fact]
    public async Task Embedded04_IdFromNonEmptyText()
    {
        var data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(new string('Z', 100)));
        var id = BlobIdV1.FromSpan(data.ToContentId().Span);
        id.IsEmbedded.ShouldBeTrue();
        id.CompAlgo.ShouldBe(BlobCompAlgo.Snappy);
        id.HashAlgo.ShouldBe(BlobHashAlgo.None);
        id.ToString().ShouldBe("S:9:ZABa/gEAigEA");
    }

}

