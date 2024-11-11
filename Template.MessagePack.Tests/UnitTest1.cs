using FluentAssertions;
using MessagePack;
using System;

using T_DomainName_.MessagePack;

namespace Template_MessagePack.Tests
{
    public class TemplateRoundtripTests
    {
        [Fact]
        public void Roundtrip()
        {
            var orig = new T_EntityName_();
            orig.T_ScalarRequiredMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_EntityName_>(orig);
            var copy = MessagePackSerializer.Deserialize<T_EntityName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            copy.Freeze();
            copy.IsFrozen().Should().BeTrue();
            copy.T_ScalarRequiredMemberName_.Should().Be(orig.T_ScalarRequiredMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }
    }
}