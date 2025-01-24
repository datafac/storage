using System;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        void PackBeforeFreeze();
        ReadOnlyMemory<ReadOnlyMemory<byte>> GetBuffers();
        void LoadBuffers(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers);
        void UnpackAfterLoad();
    }
}
