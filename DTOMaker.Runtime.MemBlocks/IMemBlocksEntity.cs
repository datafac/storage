using System;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        string GetEntityId();
        ReadOnlyMemory<ReadOnlyMemory<byte>> GetBuffers();
        void LoadBuffers(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers);
    }
}
