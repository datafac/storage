using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Runtime.Tests
{
    public class CodecRegressionTests_Int32
    {
        [Theory]
        [InlineData(1, "00-00-00-01")]
        [InlineData(0, "00-00-00-00")]
        [InlineData(-1, "FF-FF-FF-FF")]
        [InlineData(Int32.MaxValue, "7F-FF-FF-FF")]
        [InlineData(Int32.MinValue, "80-00-00-00")]
        public void Roundtrip_Int32_BE(in Int32 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_Int32_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Int32_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Int32 copy = Runtime.Codec_Int32_BE.ReadFromSpan(buffer);
#else
            Int32 copy = Runtime.Codec_Int32_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(1, "01-00-00-00")]
        [InlineData(0, "00-00-00-00")]
        [InlineData(-1, "FF-FF-FF-FF")]
        [InlineData(Int32.MaxValue, "FF-FF-FF-7F")]
        [InlineData(Int32.MinValue, "00-00-00-80")]
        public void Roundtrip_Int32_LE(in Int32 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_Int32_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Int32_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Int32 copy = Runtime.Codec_Int32_LE.ReadFromSpan(buffer);
#else
            Int32 copy = Runtime.Codec_Int32_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt32.MinValue, "00-00-00-00")]
        [InlineData(1U, "00-00-00-01")]
        [InlineData(UInt32.MaxValue, "FF-FF-FF-FF")]
        public void Roundtrip_UInt32_BE(in UInt32 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_UInt32_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_UInt32_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            UInt32 copy = Runtime.Codec_UInt32_BE.ReadFromSpan(buffer);
#else
            UInt32 copy = Runtime.Codec_UInt32_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt32.MinValue, "00-00-00-00")]
        [InlineData(1U, "01-00-00-00")]
        [InlineData(UInt32.MaxValue, "FF-FF-FF-FF")]
        public void Roundtrip_UInt32_LE(in UInt32 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_UInt32_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_UInt32_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            UInt32 copy = Runtime.Codec_UInt32_LE.ReadFromSpan(buffer);
#else
            UInt32 copy = Runtime.Codec_UInt32_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }
    }
}
