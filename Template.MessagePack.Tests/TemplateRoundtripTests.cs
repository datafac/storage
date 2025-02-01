using Shouldly;
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
            bytesRead.ShouldBe(buffer.Length);

            copy.Freeze();
            copy.IsFrozen.ShouldBeTrue();
            copy.BaseField1!.ShouldBe(orig.BaseField1);
            copy.T_RequiredScalarMemberName_.ShouldBe(orig.T_RequiredScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).ShouldBeTrue();
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
            bytesRead.ShouldBe(buffer.Length);
            recd.ShouldNotBeNull();
            recd.ShouldBeOfType<T_ConcreteEntityName_>();
            recd.Freeze();
            var copy = recd as T_ConcreteEntityName_;
            copy.ShouldNotBeNull();
            copy!.IsFrozen.ShouldBeTrue();
            copy.BaseField1!.ShouldBe(orig.BaseField1);
            copy.T_RequiredScalarMemberName_.ShouldBe(orig.T_RequiredScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).ShouldBeTrue();
        }
    }
}