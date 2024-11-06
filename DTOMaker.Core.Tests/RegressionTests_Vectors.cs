using FluentAssertions;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DTOMaker.Runtime.Tests
{
    public class RegressionTests_Vectors
    {
        [Theory]
        [InlineData(false, "FF-FF-FF-FF-FF-FF-FF-FF-01-00-00-00-00-00-00-00-FF-00-00-00-00-00-00-00")]
        [InlineData(true, "FF-FF-FF-FF-FF-FF-FF-FF-00-00-00-00-00-00-00-01-00-00-00-00-00-00-00-FF")]
        public void Array01_Int64(bool bigEndian, string expectedBuffer)
        {
            long[] orig = new long[] { -1L, 1L, 255L };
            Memory<byte> buffer = new byte[24];
            // write
            {
                ReadOnlyMemory<long> origMem = orig;
                ReadOnlySpan<long> origSpan = origMem.Span;

                // copy to buffer
                Span<long> target = MemoryMarshal.Cast<byte, long>(buffer.Span);
                target.Length.Should().Be(3);
                if (bigEndian)
                {
                    // BE encoding
                    if (BitConverter.IsLittleEndian)
                    {
                        // LE h/w
                        // mismatch - encode each element
                        for (int i = 0; i < orig.Length; i++)
                        {
                            long value = origMem.Span[i];
                            Span<byte> valueSpan = buffer.Span.Slice(i * sizeof(long), sizeof(long));
                            Codec_Int64_BE.WriteToSpan(valueSpan, value);
                        }
                    }
                    else
                    {
                        // BE h/w
                        // match - direct copy
                        origSpan.CopyTo(target);
                    }
                }
                else
                {
                    // LE encoding
                    if (BitConverter.IsLittleEndian)
                    {
                        // LE h/w
                        // match - direct copy
                        origSpan.CopyTo(target);
                    }
                    else
                    {
                        // BE h/w
                        // mismatch - encode each element
                        for (int i = 0; i < orig.Length; i++)
                        {
                            long value = origSpan[i];
                            Span<byte> valueSpan = buffer.Span.Slice(i * sizeof(long), sizeof(long));
                            Codec_Int64_LE.WriteToSpan(valueSpan, value);
                        }
                    }
                }
                string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBuffer);
            }

            // read
            {
                long[] copy = new long[orig.Length];
                Memory<long> copyMem = copy;
                Span<long> copySpan = copyMem.Span;

                ReadOnlySpan<long> source = MemoryMarshal.Cast<byte, long>(buffer.Span);
                if (bigEndian)
                {
                    // BE encoding
                    if (BitConverter.IsLittleEndian)
                    {
                        // LE h/w
                        for (int i = 0; i < orig.Length; i++)
                        {
                            ReadOnlySpan<byte> valueSpan = buffer.Span.Slice(i * sizeof(long), sizeof(long));
                            copySpan[i] = Codec_Int64_BE.ReadFromSpan(valueSpan);
                        }
                    }
                    else
                    {
                        // BE h/w
                        source.CopyTo(copySpan);
                    }
                }
                else
                {
                    // LE encoding
                    if (BitConverter.IsLittleEndian)
                    {
                        // LE h/w
                        source.CopyTo(copySpan);
                    }
                    else
                    {
                        // BE h/w
                        for (int i = 0; i < orig.Length; i++)
                        {
                            ReadOnlySpan<byte> valueSpan = buffer.Span.Slice(i * sizeof(long), sizeof(long));
                            copySpan[i] = Codec_Int64_LE.ReadFromSpan(valueSpan);
                        }
                    }

                }

                copySpan.SequenceEqual(orig.AsSpan());
            }
        }
    }
}
