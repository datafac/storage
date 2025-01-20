using System;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        ReadOnlyMemory<ReadOnlyMemory<byte>> GetBuffers();
        void LoadBuffers(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers);
    }
}
