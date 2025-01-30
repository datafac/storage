using DataFac.Storage;
using System;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public interface IMemBlocksEntity
    {
        ValueTask Pack(IDataStore dataStore);
        ReadOnlyMemory<byte> GetBuffer();
        void LoadBuffer(ReadOnlyMemory<byte> buffer);
        ValueTask Unpack(IDataStore dataStore); // todo int depth
    }
    public static class Codec_BlobId_NE
    {
        public static BlobIdV1 ReadFromSpan(ReadOnlySpan<byte> source) => new BlobIdV1(source);
        public static void WriteToSpan(Span<byte> target, BlobIdV1 value) => value.WriteTo(target);
    }
}
