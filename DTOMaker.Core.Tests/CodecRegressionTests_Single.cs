using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Runtime.Tests
{
    public class CodecRegressionTests_Single
    {
        [Theory]
        [InlineData(1F, "3F-80-00-00")]
        [InlineData(0F, "00-00-00-00")]
        [InlineData(-1F, "BF-80-00-00")]
        [InlineData(Single.MaxValue, "7F-7F-FF-FF")]
        [InlineData(Single.MinValue, "FF-7F-FF-FF")]
        [InlineData(Single.Epsilon, "00-00-00-01")]
        [InlineData(Single.PositiveInfinity, "7F-80-00-00")]
        [InlineData(Single.NegativeInfinity, "FF-80-00-00")]
        [InlineData(Single.NaN, "FF-C0-00-00")]
#if NET7_0_OR_GREATER
        [InlineData(Single.E, "40-2D-F8-54")]
        [InlineData(Single.Pi, "40-49-0F-DB")]
        [InlineData(Single.Tau, "40-C9-0F-DB")]
#endif
        public void Roundtrip_Single_BE(in Single value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_Single_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Single_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Single copy = Runtime.Codec_Single_BE.ReadFromSpan(buffer);
#else
            Single copy = Runtime.Codec_Single_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(1F, "00-00-80-3F")]
        [InlineData(0F, "00-00-00-00")]
        [InlineData(-1F, "00-00-80-BF")]
        [InlineData(Single.MaxValue, "FF-FF-7F-7F")]
        [InlineData(Single.MinValue, "FF-FF-7F-FF")]
        [InlineData(Single.Epsilon, "01-00-00-00")]
        [InlineData(Single.PositiveInfinity, "00-00-80-7F")]
        [InlineData(Single.NegativeInfinity, "00-00-80-FF")]
        [InlineData(Single.NaN, "00-00-C0-FF")]
#if NET7_0_OR_GREATER
        [InlineData(Single.E, "54-F8-2D-40")]
        [InlineData(Single.Pi, "DB-0F-49-40")]
        [InlineData(Single.Tau, "DB-0F-C9-40")]
#endif
        public void Roundtrip_Single_LE(in Single value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[4];
#if NET7_0_OR_GREATER
            Runtime.Codec_Single_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Single_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Single copy = Runtime.Codec_Single_LE.ReadFromSpan(buffer);
#else
            Single copy = Runtime.Codec_Single_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

    }
}
