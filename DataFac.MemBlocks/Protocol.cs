using DataFac.Memory;
using System;

namespace DataFac.MemBlocks
{
    public static class Protocol
    {
        /// <summary>
        /// Converts an array of buffers into a single memory block, adding a header containing
        /// the entity id and describing the number and lengths of the internal buffers.
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> CombineBuffers(string entityId, ReadOnlyMemory<byte>[] buffers)
        {
            // header structure (128 bytes)
            // part A (64 bytes) contains the UTF8-encoded entity id string
            // part B (64 bytes) contains a 16-slot array of 4-byte numbers (Int32LE)
            // - slot 0 contains the number of buffers
            // - slots 1-15 contain the lengths of the buffers
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

        public static string ParseEntityId(ReadOnlyMemory<byte> inputBuffer)
        {
            // parse header built by CombineBuffers - extract entityId
            BlockB128 headerBlock = default;
            headerBlock.TryRead(inputBuffer.Slice(0, 128).Span);
            string entityId = headerBlock.A.UTF8String;
            return entityId;
        }

        public static ReadOnlyMemory<byte>[] SplitBuffers(ReadOnlyMemory<byte> inputBuffer)
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
