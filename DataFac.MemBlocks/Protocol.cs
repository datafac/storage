using DataFac.Memory;
using System;

namespace DataFac.MemBlocks
{
    public static class Protocol
    {
        public static (long signature, long structure, Guid entityId) ParseHeader(ReadOnlyMemory<byte> inputBuffer)
        {
            // header structure (64 bytes)
            // -------------------- field map -----------------------------
            //  Seq.  Off.  Len.  N.    Type    End.  Name
            //  ----  ----  ----  ----  ------- ----  -------
            //     1     0     1        Byte    LE    MarkerByte0
            //     2     1     1        Byte    LE    MarkerByte1
            //     3     2     1        Byte    LE    HeaderMajorVersion
            //     4     3     1        Byte    LE    HeaderMinorVersion
            //     5     4     1        Byte    LE    SpareByte0
            //     6     5     1        Byte    LE    SpareByte1
            //     7     6     1        Byte    LE    SpareByte2
            //     8     7     1        Byte    LE    SpareByte3
            //     9     8     1        Byte    LE    ClassHeight
            //    10     9     1        Byte    LE    BlockSize1
            //    11    10     1        Byte    LE    BlockSize2
            //    12    11     1        Byte    LE    BlockSize3
            //    13    12     1        Byte    LE    BlockSize4
            //    14    13     1        Byte    LE    BlockSize5
            //    15    14     1        Byte    LE    BlockSize6
            //    16    15     1        Byte    LE    BlockSize7
            //    17    16    16        Guid    LE    EntityGuid
            //    18    32    16        Guid    LE    SpareGuid0
            //    19    48    16        Guid    LE    SpareGuid1
            // ------------------------------------------------------------
            var headerSpan = inputBuffer.Slice(0, 64).Span;
            BlockB064 headerBlock = default;
            headerBlock.TryRead(headerSpan);
            long signature = headerBlock.A.A.A.Int64ValueLE;
            long structure = headerBlock.A.A.B.Int64ValueLE;
            Guid entityId = headerBlock.A.B.GuidValueLE;
            return (signature, structure, entityId);
        }

        /// <summary>
        /// Converts an array of buffers into a single memory block, adding a header containing
        /// the entity id and describing the number and lengths of the internal buffers.
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> CombineBuffersOld(string entityId, ReadOnlyMemory<byte>[] buffers)
        {

            BlockB128 headerBlock = default;
            const int HeaderSlots = 16; // 16 * sizeof(int) = 64;
            Span<int> bufferLengths = stackalloc int[HeaderSlots];
            bufferLengths[0] = buffers.Length;
            for (int b = 0; b < buffers.Length; b++)
            {
                bufferLengths[b + 1] = buffers[b].Length;
            }
            headerBlock.A.UTF8String = entityId;
            headerBlock.B.SetInt32ArrayLE(bufferLengths);
            ReadOnlyMemory<byte> headerMemory = DataFac.UnsafeHelpers.BlockHelper.AsReadOnlySpan<BlockB128>(ref headerBlock).ToArray();
            // build single buffer prefixed with header
            ReadOnlySequenceBuilder<byte> builder = new ReadOnlySequenceBuilder<byte>(headerMemory);
            for (int b = 0; b < buffers.Length; b++)
            {
                builder.Add(buffers[b]);
            }
            return builder.Build().Compact();
        }

        public static ReadOnlyMemory<byte>[] SplitBuffersOld(ReadOnlyMemory<byte> inputBuffer)
        {
            // parse header built by CombineBuffers - extract buffers
            BlockB128 headerBlock = default;
            const int HeaderSlots = 16; // 16 * sizeof(int) = 64;
            headerBlock.TryRead(inputBuffer.Slice(0, 128).Span);
            int readPosition = 128;
            Span<int> bufferLengths = stackalloc int[HeaderSlots];
            headerBlock.B.GetInt32ArrayLE(bufferLengths);
            int bufferCount = bufferLengths[0];
            ReadOnlyMemory<byte>[] buffers = new ReadOnlyMemory<byte>[bufferCount];
            for (int b = 0; b < bufferCount; b++)
            {
                int bufferLength = bufferLengths[b + 1];
                buffers[b] = inputBuffer.Slice(readPosition, bufferLength);
                readPosition += bufferLength;
            }
            return buffers;
        }
    }
}
