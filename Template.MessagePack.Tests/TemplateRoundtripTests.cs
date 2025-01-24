using FluentAssertions;
using MessagePack;
using System;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete

using T_NameSpace_.MessagePack;

namespace Template_MessagePack.Tests
{
    public class TemplateRoundtripTests
    {
        [Fact]
        public void Roundtrip01AsEntity()
        {
            var orig = new T_ConcreteEntityName_();
            orig.BaseField1 = 321;
            orig.T_RequiredScalarMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_ConcreteEntityName_>(orig);
            var copy = MessagePackSerializer.Deserialize<T_ConcreteEntityName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            copy.Freeze();
            copy.IsFrozen.Should().BeTrue();
            copy.BaseField1!.Should().Be(orig.BaseField1);
            copy.T_RequiredScalarMemberName_.Should().Be(orig.T_RequiredScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }

        [Fact]
        public void Roundtrip03AsBase()
        {
            var orig = new T_ConcreteEntityName_();
            orig.BaseField1 = 321;
            orig.T_RequiredScalarMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_BaseNameSpace_.MessagePack.T_BaseName_>(orig);
            var recd = MessagePackSerializer.Deserialize<T_BaseNameSpace_.MessagePack.T_BaseName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);
            recd.Should().NotBeNull();
            recd.Should().BeOfType<T_ConcreteEntityName_>();
            recd.Freeze();
            var copy = recd as T_ConcreteEntityName_;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.BaseField1!.Should().Be(orig.BaseField1);
            copy.T_RequiredScalarMemberName_.Should().Be(orig.T_RequiredScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }
    }
}