using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Runtime.Tests
{
    public class CodecRegressionTests_Double
    {
        [Theory]
        [InlineData(1D, "3F-F0-00-00-00-00-00-00")]
        [InlineData(0D, "00-00-00-00-00-00-00-00")]
        [InlineData(-1D, "BF-F0-00-00-00-00-00-00")]
        [InlineData(double.MaxValue, "7F-EF-FF-FF-FF-FF-FF-FF")]
        [InlineData(double.MinValue, "FF-EF-FF-FF-FF-FF-FF-FF")]
        [InlineData(double.Epsilon, "00-00-00-00-00-00-00-01")]
        [InlineData(double.PositiveInfinity, "7F-F0-00-00-00-00-00-00")]
        [InlineData(double.NegativeInfinity, "FF-F0-00-00-00-00-00-00")]
        [InlineData(double.NaN, "FF-F8-00-00-00-00-00-00")]
#if NET7_0_OR_GREATER
        [InlineData(double.E, "40-05-BF-0A-8B-14-57-69")]
        [InlineData(double.Pi, "40-09-21-FB-54-44-2D-18")]
        [InlineData(double.Tau, "40-19-21-FB-54-44-2D-18")]
#endif
        public void Roundtrip_Double_BE(in double value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
#if NET7_0_OR_GREATER
            Runtime.Codec_Double_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Double_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            double copy = Runtime.Codec_Double_BE.ReadFromSpan(buffer);
#else
            double copy = Runtime.Codec_Double_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData(1D, "00-00-00-00-00-00-F0-3F")]
        [InlineData(0D, "00-00-00-00-00-00-00-00")]
        [InlineData(-1D, "00-00-00-00-00-00-F0-BF")]
        [InlineData(double.MaxValue, "FF-FF-FF-FF-FF-FF-EF-7F")]
        [InlineData(double.MinValue, "FF-FF-FF-FF-FF-FF-EF-FF")]
        [InlineData(double.Epsilon, "01-00-00-00-00-00-00-00")]
        [InlineData(double.PositiveInfinity, "00-00-00-00-00-00-F0-7F")]
        [InlineData(double.NegativeInfinity, "00-00-00-00-00-00-F0-FF")]
        [InlineData(double.NaN, "00-00-00-00-00-00-F8-FF")]
#if NET7_0_OR_GREATER
        [InlineData(double.E, "69-57-14-8B-0A-BF-05-40")]
        [InlineData(double.Pi, "18-2D-44-54-FB-21-09-40")]
        [InlineData(double.Tau, "18-2D-44-54-FB-21-19-40")]
#endif
        public void Roundtrip_Double_LE(in double value, string expectedBytes)
        {
            Span<byte> buffer = stackalloc byte[8];
#if NET7_0_OR_GREATER
            Runtime.Codec_Double_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Double_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            double copy = Runtime.Codec_Double_LE.ReadFromSpan(buffer);
#else
            double copy = Runtime.Codec_Double_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

    }
}
