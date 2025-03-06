using Shouldly;
using Xunit;
using DataFac.Storage;

namespace DTOMaker.MemBlocks.Tests
{
    public class BlobIdV1Checks
    {
        [Fact]
        public void CheckBlobIdSize()
        {
            BlobIdV1.Size.ShouldBe(Constants.BlobIdV1Size);
        }

        [Fact]
        public void CheckStructureCode1()
        {
            var sc = new StructureCode(1, 8);
            sc.Bits.ShouldBe(0x0041L);
        }

        [Fact]
        public void CheckStructureCode2()
        {
            var sc = new StructureCode(1, 8)
                .AddStructure(2, 32);
            sc.Bits.ShouldBe(0x0641L);
        }

        [Fact]
        public void CheckStructureCode3()
        {
            var sc = new StructureCode(1, 8)
                .AddStructure(2, 32)
                .AddStructure(3, 1024);
            sc.Bits.ShouldBe(0xB641L);
        }
    }
}