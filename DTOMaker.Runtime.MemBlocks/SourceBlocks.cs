using DataFac.Memory;
using System;
using System.Buffers;

namespace DTOMaker.Runtime.MemBlocks
{
    public readonly struct SourceBlocks
    {
        public readonly BlockHeader Header;
        public readonly int ClassHeight;
        private readonly ReadOnlyMemory<ReadOnlyMemory<byte>> _blocks;
        public ReadOnlyMemory<byte> GetBlock(int classHeight) => _blocks.Span[classHeight];

        private SourceBlocks(BlockHeader header, int classHeight, ReadOnlyMemory<ReadOnlyMemory<byte>> blocks)
        {
            Header = header;
            ClassHeight = classHeight;
            _blocks = blocks;
        }

        private static readonly int[] _blockSizes = [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8, 1024 * 16, 1024 * 32];

        public static SourceBlocks ParseFrom(System.Buffers.ReadOnlySequence<byte> buffers)
        {
            int startPosition = 0;
            ReadOnlyMemory<byte> headerMemory = buffers.Slice(startPosition, 64).Compact();
            startPosition += 64;

            // parse header
            BlockHeader header = BlockHeader.ParseFrom(headerMemory);

            // get remaining blocks
            // if the source is a single element or the source elements match the target
            // structure, then the slice compactions will not allocate new memory.
            long bits = header.StructureBits;
            int classHeight = (int)(bits & 0x0F);
            ReadOnlyMemory<byte>[] blocks = new ReadOnlyMemory<byte>[classHeight + 1];
            blocks[0] = headerMemory;
            for (int h = 0; h < classHeight && h < 15; h++)
            {
                bits = bits >> 4;
                int blockLength = _blockSizes[(int)(bits & 0x0F)];
                ReadOnlyMemory<byte> block = buffers.Slice(startPosition, blockLength).Compact();
                startPosition += blockLength;
                blocks[h + 1] = block;
            }

            return new SourceBlocks(header, classHeight, blocks);
        }
    }
}
