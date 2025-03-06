using Shouldly;
using Xunit;
using DataFac.Storage;
using DTOMaker.Runtime.MemBlocks;
using System;
using DataFac.Memory;

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
            var sc = new StructureCode(1, 16);
            sc.Bits.ShouldBe(0x0051);
            sc.Bits.ShouldBe(81L);
        }

        [Fact]
        public void CheckStructureCode2()
        {
            var sc = new StructureCode(2, 0)
                .AddInnerBlock(1, 16);
            sc.Bits.ShouldBe(0x0052);
            sc.Bits.ShouldBe(82L);
        }

        [Fact]
        public void CheckStructureCode3()
        {
            var sc = new StructureCode(3, 8)
                .AddInnerBlock(2, 0)
                .AddInnerBlock(1, 16);
            sc.Bits.ShouldBe(0x4053);
            sc.Bits.ShouldBe(16467L);
        }

        [Fact]
        public void CheckSourceBlocks()
        {
            Guid id = new Guid("42b6dc23-d54d-4e68-9688-2290bb9d3168");
            var sc = new StructureCode(3, 8)
                .AddInnerBlock(2, 0)
                .AddInnerBlock(1, 16);
            sc.Bits.ShouldBe(0x4053);
            sc.Bits.ShouldBe(16467L);

            var header = BlockHeader.CreateNew(sc.Bits, id);

            ReadOnlySequenceBuilder<byte> builder = new ReadOnlySequenceBuilder<byte>(header.Memory);
            builder = builder.Append(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF });
            builder = builder.Append(Array.Empty<byte>());
            builder = builder.Append(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            var buffers = builder.Build();
            SourceBlocks blocks = SourceBlocks.ParseFrom(buffers);

            blocks.Header.Memory.Span[0].ShouldBe<byte>(0x7C);
            blocks.Header.Memory.Span[1].ShouldBe<byte>(0x5F);
            blocks.Header.Memory.Span[2].ShouldBe<byte>(0x01);
            blocks.Header.Memory.Span[3].ShouldBe<byte>(0x00);
            blocks.Header.SignatureBits.ShouldBe(0x00015F7C);
            blocks.Header.StructureBits.ShouldBe(0x00004053);
            blocks.Header.EntityGuid.ShouldBe(id);
            blocks.ClassHeight.ShouldBe(3);
            blocks.Blocks.Span[0].Length.ShouldBe(64);
            blocks.Blocks.Span[1].Length.ShouldBe(16);
            blocks.Blocks.Span[2].Length.ShouldBe(0);
            blocks.Blocks.Span[3].Length.ShouldBe(8);
        }

    }
}