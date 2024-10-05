using DTOMaker.Runtime;
using FluentAssertions;
using System;
using System.Linq;

namespace DTOMaker.Core.Tests
{
    public class CodecRegressionTests_Guid
    {
        private static Guid GetTestGuid(string input)
        {
            return input switch
            {
                "empty" => Guid.Empty,
                "test1" => new Guid(1, 2, 3, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7),
                "test2" => new Guid("ed6c5a09-2aad-4701-b36d-adef00f267c3"),
                _ => Guid.Parse(input),
            };
        }

        [Theory]
        [InlineData("empty", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("test1", "00-00-00-01-00-02-00-03-F0-F1-F2-F3-F4-F5-F6-F7", "01-00-00-00-02-00-03-00-F0-F1-F2-F3-F4-F5-F6-F7")]
        [InlineData("test2", "ED-6C-5A-09-2A-AD-47-01-B3-6D-AD-EF-00-F2-67-C3", "09-5A-6C-ED-AD-2A-01-47-B3-6D-AD-EF-00-F2-67-C3")]
        public void GuidConverterCheck(string input, string bigEndianText, string littleEndianText)
        {
            Guid orig = GetTestGuid(input);
            {
                Span<byte> buffer = stackalloc byte[16];
                GuidHelper.WriteToSpan(buffer, true, orig);
                string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(bigEndianText);
                Guid copy = GuidHelper.ReadFromSpan(buffer, true);
                copy.Should().Be(orig);
            }

            {
                Span<byte> buffer = stackalloc byte[16];
                GuidHelper.WriteToSpan(buffer, false, orig);
                string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(littleEndianText);
                Guid copy = GuidHelper.ReadFromSpan(buffer, false);
                copy.Should().Be(orig);
            }
        }

        [Theory]
        [InlineData("empty", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("test1", "00-00-00-01-00-02-00-03-F0-F1-F2-F3-F4-F5-F6-F7")]
        [InlineData("test2", "ED-6C-5A-09-2A-AD-47-01-B3-6D-AD-EF-00-F2-67-C3")]
        public void Roundtrip_Guid_BE(string input, string expectedBytes)
        {
            Guid value = GetTestGuid(input);
            Span<byte> buffer = stackalloc byte[16];
#if NET7_0_OR_GREATER
            Runtime.Codec_Guid_BE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Guid_BE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Guid copy = Runtime.Codec_Guid_BE.ReadFromSpan(buffer);
#else
            Guid copy = Runtime.Codec_Guid_BE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

        [Theory]
        [InlineData("empty", "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")]
        [InlineData("test1", "01-00-00-00-02-00-03-00-F0-F1-F2-F3-F4-F5-F6-F7")]
        [InlineData("test2", "09-5A-6C-ED-AD-2A-01-47-B3-6D-AD-EF-00-F2-67-C3")]
        public void Roundtrip_Guid_LE(string input, string expectedBytes)
        {
            Guid value = GetTestGuid(input);
            Span<byte> buffer = stackalloc byte[16];
#if NET7_0_OR_GREATER
            Runtime.Codec_Guid_LE.WriteToSpan(buffer, value);
#else
            Runtime.Codec_Guid_LE.Instance.WriteTo(buffer, value);
#endif
            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);
#if NET7_0_OR_GREATER
            Guid copy = Runtime.Codec_Guid_LE.ReadFromSpan(buffer);
#else
            Guid copy = Runtime.Codec_Guid_LE.Instance.ReadFrom(buffer);
#endif
            copy.Should().Be(value);
        }

    }
}
