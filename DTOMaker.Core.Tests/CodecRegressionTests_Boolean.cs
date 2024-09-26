using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Core.Tests
{
    public class CodecRegressionTests_Boolean
    {
        [Theory]
        [InlineData(false, "00")]
        [InlineData(true, "01")]
        public void Roundtrip_Int16_BE(in Boolean value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_Boolean_BE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Boolean copy = Runtime.Codec_Boolean_BE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(false, "00")]
        [InlineData(true, "01")]
        public void Roundtrip_Int16_LE(in Boolean value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[1];
            Runtime.Codec_Boolean_LE.Instance.WriteTo(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Boolean copy = Runtime.Codec_Boolean_LE.Instance.ReadFrom(buffer);
            copy.Should().Be(value);
        }

    }
}
