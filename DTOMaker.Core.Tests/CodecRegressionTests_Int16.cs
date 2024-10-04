using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Core.Tests
{
    public class CodecRegressionTests_Int16
    {
        [Theory]
        [InlineData((Int16)1, "00-01")]
        [InlineData((Int16)0, "00-00")]
        [InlineData((Int16)(-1), "FF-FF")]
        [InlineData(Int16.MaxValue, "7F-FF")]
        [InlineData(Int16.MinValue, "80-00")]
        public void Roundtrip_Int16_BE(in Int16 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[2];
#if NET7_0_OR_GREATER
            Runtime.Codec_Int16_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Int16_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Int16 copy = Runtime.Codec_Int16_BE.ReadFromSpan(buffer);
#else
            Int16 copy = Runtime.Codec_Int16_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData((Int16)1, "01-00")]
        [InlineData((Int16)0, "00-00")]
        [InlineData((Int16)(-1), "FF-FF")]
        [InlineData(Int16.MaxValue, "FF-7F")]
        [InlineData(Int16.MinValue, "00-80")]
        public void Roundtrip_Int16_LE(in Int16 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[2];
#if NET7_0_OR_GREATER
            Runtime.Codec_Int16_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Int16_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Int16 copy = Runtime.Codec_Int16_LE.ReadFromSpan(buffer);
#else
            Int16 copy = Runtime.Codec_Int16_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt16.MinValue, "00-00")]
        [InlineData((UInt16)1, "00-01")]
        [InlineData(UInt16.MaxValue, "FF-FF")]
        public void Roundtrip_UInt16_BE(in UInt16 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[2];
#if NET7_0_OR_GREATER
            Runtime.Codec_UInt16_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_UInt16_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            UInt16 copy = Runtime.Codec_UInt16_BE.ReadFromSpan(buffer);
#else
            UInt16 copy = Runtime.Codec_UInt16_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt16.MinValue, "00-00")]
        [InlineData((UInt16)1, "01-00")]
        [InlineData(UInt16.MaxValue, "FF-FF")]
        public void Roundtrip_UInt16_LE(in UInt16 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[2];
#if NET7_0_OR_GREATER
            Runtime.Codec_UInt16_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_UInt16_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            UInt16 copy = Runtime.Codec_UInt16_LE.ReadFromSpan(buffer);
#else
            UInt16 copy = Runtime.Codec_UInt16_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

    }
}
