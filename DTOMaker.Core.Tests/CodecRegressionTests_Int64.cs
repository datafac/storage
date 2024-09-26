using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Core.Tests
{
    public class CodecRegressionTests_Int64
    {
        [Theory]
        [InlineData(1L, "00-00-00-00-00-00-00-01")]
        [InlineData(0L, "00-00-00-00-00-00-00-00")]
        [InlineData(-1L, "FF-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData(Int64.MaxValue, "7F-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData(Int64.MinValue, "80-00-00-00-00-00-00-00")]
        public void Roundtrip_Int64_BE(in Int64 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
            Runtime.Codec_Int64_BE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Int64 copy = Runtime.Codec_Int64_BE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(1L, "01-00-00-00-00-00-00-00")]
        [InlineData(0L, "00-00-00-00-00-00-00-00")]
        [InlineData(-1L, "FF-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData(Int64.MaxValue, "FF-FF-FF-FF-FF-FF-FF-7F")]
        [InlineData(Int64.MinValue, "00-00-00-00-00-00-00-80")]
        public void Roundtrip_Int64_LE(in Int64 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
            Runtime.Codec_Int64_LE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Int64 copy = Runtime.Codec_Int64_LE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt64.MinValue, "00-00-00-00-00-00-00-00")]
        [InlineData(1UL, "00-00-00-00-00-00-00-01")]
        [InlineData(UInt64.MaxValue, "FF-FF-FF-FF-FF-FF-FF-FF")]
        public void Roundtrip_UInt64_BE(in UInt64 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
            Runtime.Codec_UInt64_BE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            UInt64 copy = Runtime.Codec_UInt64_BE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(UInt64.MinValue, "00-00-00-00-00-00-00-00")]
        [InlineData(1UL, "01-00-00-00-00-00-00-00")]
        [InlineData(UInt64.MaxValue, "FF-FF-FF-FF-FF-FF-FF-FF")]
        public void Roundtrip_UInt64_LE(in UInt64 value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
            Runtime.Codec_UInt64_LE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            UInt64 copy = Runtime.Codec_UInt64_LE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

    }
}
