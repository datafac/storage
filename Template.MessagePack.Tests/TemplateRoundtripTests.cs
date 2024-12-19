using FluentAssertions;
using MessagePack;
using System;
using Xunit;

using T_DomainName_.MessagePack;

namespace Template_MessagePack.Tests
{
    public class TemplateRoundtripTests
    {
        [Fact]
        public void Roundtrip01AsEntity()
        {
            var orig = new T_EntityName_();
            orig.BaseField1 = 321;
            orig.T_ScalarRequiredMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_EntityName_>(orig);
            var copy = MessagePackSerializer.Deserialize<T_EntityName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            copy.Freeze();
            copy.IsFrozen.Should().BeTrue();
            copy.BaseField1!.Should().Be(orig.BaseField1);
            copy.T_ScalarRequiredMemberName_.Should().Be(orig.T_ScalarRequiredMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }

        [Fact]
        public void Roundtrip02AsBase()
        {
            var orig = new T_EntityName_();
            orig.BaseField1 = 321;
            orig.T_ScalarRequiredMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<EntityBase>(orig);
            var recd = MessagePackSerializer.Deserialize<EntityBase>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);
            recd.Should().NotBeNull();
            recd.Should().BeOfType<T_EntityName_>();
            recd.Freeze();
            var copy = recd as T_EntityName_;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.BaseField1!.Should().Be(orig.BaseField1);
            copy.T_ScalarRequiredMemberName_.Should().Be(orig.T_ScalarRequiredMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }

        [Fact]
        public void Roundtrip03AsParent()
        {
            var orig = new T_EntityName_();
            orig.BaseField1 = 321;
            orig.T_ScalarRequiredMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_BaseName_>(orig);
            var recd = MessagePackSerializer.Deserialize<T_BaseName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);
            recd.Should().NotBeNull();
            recd.Should().BeOfType<T_EntityName_>();
            recd.Freeze();
            var copy = recd as T_EntityName_;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.BaseField1!.Should().Be(orig.BaseField1);
            copy.T_ScalarRequiredMemberName_.Should().Be(orig.T_ScalarRequiredMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }
    }
}