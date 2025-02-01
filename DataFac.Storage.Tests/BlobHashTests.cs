using Xunit;
using Shouldly;
using System.Linq;
using DataFac.Storage;

namespace DataFac.Storage.Tests;

public class BlobHashTests
{
    [Fact]
    public void BlobHash01Empty()
    {
        BlobData orig = default;
        BlobIdV1 id = orig.GetId();
        id.ToString().ShouldBe("V1.0:0:0:0:1:47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
    }

    [Fact]
    public void BlobHash02NonEmpty()
    {
        BlobData orig = new BlobData(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = orig.GetId();
        id.ToString().ShouldBe("V1.0:256:0:0:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");
    }
}
