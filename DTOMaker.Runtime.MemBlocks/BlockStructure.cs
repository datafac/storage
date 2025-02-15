using DataFac.Memory;
using System;

namespace DTOMaker.Runtime.MemBlocks
{
    /// <summary>
    /// todo BlockStructure tests
    /// </summary>
    public readonly struct BlockStructure
    {
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

        public readonly int EffectiveLength;

        public BlockStructure(long signature, long structure, Guid entityGuid)
        {
            SignatureBits = signature;
            StructureBits = structure;
            EntityGuid = entityGuid;
            EffectiveLength = GetEffectiveLength(structure);
        }

        public BlockStructure(ReadOnlySpan<byte> source)
        {
            SignatureBits = Codec_Int64_LE.ReadFromSpan(source.Slice(0, 8));
            StructureBits = Codec_Int64_LE.ReadFromSpan(source.Slice(8, 8));
            EntityGuid = Codec_Guid_LE.ReadFromSpan(source.Slice(16, 16));
            EffectiveLength = GetEffectiveLength(StructureBits);
        }

        public void WriteTo(Span<byte> target)
        {
            Codec_Int64_LE.WriteToSpan(target.Slice(0, 8), SignatureBits);
            Codec_Int64_LE.WriteToSpan(target.Slice(8, 8), StructureBits);
            Codec_Guid_LE.WriteToSpan(target.Slice(16, 16), EntityGuid);
        }

        private static readonly int[] _blockSizes = [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8, 1024 * 16, 1024 * 32];
        private static int GetEffectiveBlockSize(int code)
        {
            ReadOnlySpan<int> blockSizes = _blockSizes;
            return Math.Max(64, blockSizes[code]);
        }

        private static int GetEffectiveLength(long structureCode)
        {
            int classHeight = (int)(structureCode & 0x0F);
            int totalLength = 64;
            long bits = structureCode;
            for (int h = 0; h < classHeight && h < 15; h++)
            {
                bits = bits >> 4;
                int blockLength = GetEffectiveBlockSize((int)(bits & 0x0F));
                totalLength += blockLength;
            }
            return totalLength;
        }

    }
}
