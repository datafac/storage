using Xunit;
using FluentAssertions;
using System.Linq;
using Inventory.Store;

namespace Inventory.Tests;

public class BlobHashTests
{
    [Fact]
    public void BlobHash01Empty()
    {
        BlobData orig = default;
        BlobId id = orig.GetBlobId();
        id.ToString().Should().Be("V1/E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
    }

    [Fact]
    public void BlobHash02NonEmpty()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobId id = orig.GetBlobId();
        id.ToString().Should().Be("V1/40AFF2E9D2D8922E47AFD4648E6967497158785FBD1DA870E7110266BF944880");
    }
}
