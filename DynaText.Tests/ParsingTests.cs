using Argon;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.Gentime.Tests
{
    public class ParsingTests
    {
        [Theory]
        [InlineData(0, """ """)]
        [InlineData(1, """{}""")]
        [InlineData(2, """{A=1,B=2}""")]
        [InlineData(3, """{A=[],B=[1],C=[1,2]}""")]
        [InlineData(4, """{A={X=1,Y=2}}""")]
        public void Parse0_Success_InputIsValid(int _, string input)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
        }

        [Theory]
        [InlineData(0, """{""", "Unexpected EOF.")]
        [InlineData(1, """A=1,B=2}""", "Unexpected token: Identifier(A) found at (L0,C0).")]
        [InlineData(2, """{A=[],B=[1,C=[1,2]}""", "Unexpected identifier: 'C' found at (L0,C11).")]
        [InlineData(3, """{A={X=1,Y=2}""", "Unexpected EOF.")]
        [InlineData(4, """{[}]""", "Unexpected token: LeftSquare([) found at (L0,C1).")]
        public void Parse1_Failures_UnbalancedInput(int _, string input, string expectedError)
        {
            using var reader = new StringReader(input);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBe(expectedError);
        }

        [Fact]
        public async Task Roundtrip0_Empty()
        {
            DynaMap orig = new DynaMap();

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1a_Shallow_Bool()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", true);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1b_Shallow_Int64()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", 123456L);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1c_Shallow_UInt64()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", 123456UL);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1d_Shallow_String()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", "abcdef");

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1e_Shallow_Null()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", null);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1f_Shallow_Byte()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", (byte)123);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1g_Shallow_SByte()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", (sbyte)123);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1h_Shallow_Int32()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", 123456);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1i_Shallow_UInt32()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", 123456U);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1j_Shallow_Int16()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", (short)12345);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Roundtrip1k_Shallow_UInt16()
        {
            DynaMap orig = new DynaMap();
            orig.Add("Field1", (ushort)12345);

            using var writer = new StringWriter();
            orig.Emit(writer, 0);
            string buffer = writer.ToString();

            await Verifier.Verify(buffer);

            using var reader = new StringReader(buffer);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            result.Message.ShouldBeNull();
            result.IsError.ShouldBeFalse();
            result.Consumed.ShouldBeGreaterThan(0);

            DynaMap? copy = result.Output as DynaMap;

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<DynaMap>();
            copy.ShouldBe(orig);
        }

    }
}