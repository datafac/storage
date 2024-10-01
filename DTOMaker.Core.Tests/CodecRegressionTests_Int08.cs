using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Core.Tests
{
    public class CodecRegressionTests_Int08
    {
        [Theory]
        [InlineData((SByte)1, "01")]
        [InlineData((SByte)0, "00")]
        [InlineData((SByte)(-1), "FF")]
        [InlineData(SByte.MaxValue, "7F")]
        [InlineData(SByte.MinValue, "80")]
        public void Roundtrip_SByte_BE(in SByte value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_SByte_BE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            SByte copy = Runtime.Codec_SByte_BE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData((SByte)1, "01")]
        [InlineData((SByte)0, "00")]
        [InlineData((SByte)(-1), "FF")]
        [InlineData(SByte.MaxValue, "7F")]
        [InlineData(SByte.MinValue, "80")]
        public void Roundtrip_SByte_LE(in SByte value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_SByte_LE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            SByte copy = Runtime.Codec_SByte_LE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(Byte.MinValue, "00")]
        [InlineData((Byte)1, "01")]
        [InlineData(Byte.MaxValue, "FF")]
        public void Roundtrip_Byte_BE(in Byte value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_Byte_BE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Byte copy = Runtime.Codec_Byte_BE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(Byte.MinValue, "00")]
        [InlineData((Byte)1, "01")]
        [InlineData(Byte.MaxValue, "FF")]
        public void Roundtrip_Byte_LE(in Byte value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_Byte_LE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Byte copy = Runtime.Codec_Byte_LE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

    }
}
