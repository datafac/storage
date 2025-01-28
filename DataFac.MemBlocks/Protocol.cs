using DataFac.Memory;
using System;

namespace DataFac.MemBlocks
{
    public static class Protocol
    {
        /// <summary>
        /// Converts an array of buffers into a single memory block, adding a header describing
        /// the number and lengths of the buffers.
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> CombineBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            // build header
            // - slot 0 contains the number of buffers
            // - slots 1-15 contain the lengths of the buffers
            BlockB064 headerBlock = default;
            const int HeaderSlots = 16; // 16 * sizeof(int) = 64;
            Span<int> bufferLengths = stackalloc int[HeaderSlots];
            bufferLengths[0] = buffers.Length;
            for (int b = 0; b < buffers.Length; b++)
            {
                bufferLengths[b + 1] = buffers[b].Length;
            }
            headerBlock.SetInt32ArrayLE(bufferLengths);
            ReadOnlyMemory<byte> headerMemory = DataFac.UnsafeHelpers.BlockHelper.AsReadOnlySpan<BlockB064>(ref headerBlock).ToArray();
            // build single buffer prefixed with header
            ReadOnlySequenceBuilder<byte> builder = new ReadOnlySequenceBuilder<byte>(headerMemory);
            for (int b = 0; b < buffers.Length; b++)
            {
                builder.Add(buffers[b]);
            }
            return builder.Build().Compact();
        }

        public static ReadOnlyMemory<byte>[] SplitBuffers(ReadOnlyMemory<byte> inputBuffer)
        {
            // get header
            // - slot 0 contains the number of buffers
            // - slots 1-15 contain the lengths of the buffers
            BlockB064 headerBlock = default;
            const int HeaderSlots = 16; // 16 * sizeof(int) = 64;
            headerBlock.TryRead(inputBuffer.Slice(0, 64).Span);
            int readPosition = 64;
            Span<int> bufferLengths = stackalloc int[HeaderSlots];
            headerBlock.GetInt32ArrayLE(bufferLengths);
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
