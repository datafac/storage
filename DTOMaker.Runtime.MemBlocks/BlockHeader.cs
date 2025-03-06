using DataFac.Memory;
using System;

namespace DTOMaker.Runtime.MemBlocks
{
    public static class Constants
    {
        public const int HeaderSize = 64;
    }
    public readonly struct BlockHeader
    {
        public static BlockHeader CreateNew(long structureBits, Guid entityGuid)
        {
            Memory<byte> memory = new byte[Constants.HeaderSize];
            Span<byte> headerSpan = memory.Span;
            Codec_Int64_LE.WriteToSpan(headerSpan.Slice(0, 8), SignatureBitsV10);
            Codec_Int64_LE.WriteToSpan(headerSpan.Slice(8, 8), structureBits);
            Codec_Guid_LE.WriteToSpan(headerSpan.Slice(16, 16), entityGuid);
            return new BlockHeader(SignatureBitsV10, structureBits, entityGuid, memory);
        }

        public static BlockHeader ParseFrom(ReadOnlyMemory<byte> buffer)
        {
            var header = buffer.Slice(0, Constants.HeaderSize);
            var signature = Codec_Int64_LE.ReadFromSpan(header.Span.Slice(0, 8));
            // todo check signature marker and version bytes
            var structureBits = Codec_Int64_LE.ReadFromSpan(header.Span.Slice(8, 8));
            var entityGuid = Codec_Guid_LE.ReadFromSpan(header.Span.Slice(16, 16));
            return new BlockHeader(signature, structureBits, entityGuid, header);
        }

        private const long SignatureBitsV10 = 89980L;

        /// <summary>
        /// Marker and version bytes
        /// </summary>
        public readonly long SignatureBits;

        /// <summary>
        /// Class height and block size codes
        /// </summary>
        public readonly long StructureBits;

        /// <summary>
        /// Entity identifier
        /// </summary>
        public readonly Guid EntityGuid;

        public readonly ReadOnlyMemory<byte> Memory;

        private BlockHeader(long signatureBits, long structureBits, Guid entityGuid, ReadOnlyMemory<byte> memory)
        {
            SignatureBits = signatureBits;
            StructureBits = structureBits;
            EntityGuid = entityGuid;
            Memory = memory;
        }

        public bool Equals(BlockHeader other)
        {
            if (other.SignatureBits != SignatureBits) return false;
            if (other.StructureBits != StructureBits) return false;
            if (other.EntityGuid != EntityGuid) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is BlockHeader other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(SignatureBits, StructureBits, EntityGuid);
        public static bool operator ==(BlockHeader left, BlockHeader right) => left.Equals(right);
        public static bool operator !=(BlockHeader left, BlockHeader right) => !left.Equals(right);
    }
}
