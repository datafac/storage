using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Runtime.Tests
{
#if NET8_0_OR_GREATER
    public class CodecRegressionTests_Int128
    {
        private static Int128 GetTestInt128(string input)
        {
            return input switch
            {
                "max" => Int128.MaxValue,
                "min" => Int128.MinValue,
                _ => Int128.Parse(input),
            };
        }

        private static UInt128 GetTestUInt128(string input)
        {
            return input switch
            {
                "max" => UInt128.MaxValue,
                "min" => UInt128.MinValue,
                _ => UInt128.Parse(input),
            };
        }

        [Theory]
        [InlineData("1", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01")]
        [InlineData("0",   "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("-1",  "FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData("max", "7F-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData("min", "80-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        public void Roundtrip_Int128_BE(string input, string expectedBytes)
        {
            Int128 value = GetTestInt128(input);
            Span<byte> buffer = stackalloc byte[16];
            Runtime.Codec_Int128_BE.WriteToSpan(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Int128 copy = Runtime.Codec_Int128_BE.ReadFromSpan(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData("1", "01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("0", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("-1", "FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF")]
        [InlineData("max", "FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-7F")]
        [InlineData("min", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-80")]
        public void Roundtrip_Int128_LE(string input, string expectedBytes)
        {
            Int128 value = GetTestInt128(input);
            Span<byte> buffer = stackalloc byte[16];
            Runtime.Codec_Int128_LE.WriteToSpan(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            Int128 copy = Runtime.Codec_Int128_LE.ReadFromSpan(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData("min", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("1", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01")]
        [InlineData("max", "FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF")]
        public void Roundtrip_UInt128_BE(string input, string expectedBytes)
        {
            UInt128 value = GetTestUInt128(input);
            Span<byte> buffer = stackalloc byte[16];
            Runtime.Codec_UInt128_BE.WriteToSpan(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            UInt128 copy = Runtime.Codec_UInt128_BE.ReadFromSpan(buffer);
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData("min", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("1", "01-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("max", "FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF-FF")]
        public void Roundtrip_UInt128_LE(string input, string expectedBytes)
        {
            UInt128 value = GetTestUInt128(input);
            Span<byte> buffer = stackalloc byte[16];
            Runtime.Codec_UInt128_LE.WriteToSpan(buffer, value);
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
            UInt128 copy = Runtime.Codec_UInt128_LE.ReadFromSpan(buffer);
            copy.Should().Be(value);
        }
    }
#endif
}
