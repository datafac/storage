using DataFac.Memory;
using DTOMaker.Runtime.MemBlocks;
using Shouldly;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Template.MemBlocks.Tests
{
    internal sealed class TestEntity : EntityBase
    {
        private static readonly Guid _entityGuid = new Guid("8bb1290a-9336-4371-8dad-fccd8d9c4494");
        private static readonly BlockStructure _structure = new BlockStructure(89980L, 0x61, _entityGuid);
        protected override int OnGetClassHeight() => 1;
        protected override string OnGetEntityId() => _entityGuid.ToString("D");
        public TestEntity() : base(_structure)
        {
        }
    }
    public class EntityBaseTests
    {
        [Fact]
        public void ParseBlockHeader()
        {
            BlockB064 header = default;
            // signature
            header.A.A.A.A.A.A.ByteValue = (byte)'|';
            header.A.A.A.A.A.B.ByteValue = (byte)'_';
            header.A.A.A.A.B.A.ByteValue = (byte)1;
            header.A.A.A.A.B.B.ByteValue = (byte)0;
            // structure
            header.A.A.B.Int64ValueLE = 0x61;
            // entityid
            header.A.B.GuidValueLE = new Guid("aa1e2d7b-fcb5-4739-9624-f6a648815251");

            Span<byte> buffer = stackalloc byte[64];
            bool written = header.TryWrite(buffer);
            written.ShouldBeTrue();

            BlockStructure structure = new BlockStructure(buffer);
            structure.SignatureBits.ShouldBe(89980L);
            structure.StructureBits.ShouldBe(0x61);
            structure.EffectiveLength.ShouldBe(128);
            structure.EntityGuid.ToString("D").ShouldBe("aa1e2d7b-fcb5-4739-9624-f6a648815251");
        }
        [Fact]
        public async Task BlockHeaderIsConstant()
        {
            using var dataStore = new DataFac.Storage.Testing.TestDataStore();
            var orig = new TestEntity();
            await orig.Pack(dataStore);
            orig.Freeze();
            var buffer = orig.GetBuffer().Span;
            buffer.Length.ShouldBe(128);

            buffer[0].ShouldBe((byte)'|');  // marker byte 0
            buffer[1].ShouldBe((byte)'_');  // marker byte 1
            buffer[2].ShouldBe((byte)1);    // major version
            buffer[3].ShouldBe((byte)0);    // minor version

            BlockStructure structure = new BlockStructure(buffer);
            structure.SignatureBits.ShouldBe(89980L);
            structure.StructureBits.ShouldBe(0x61);
            structure.EffectiveLength.ShouldBe(128);
            structure.EntityGuid.ToString("D").ShouldBe("8bb1290a-9336-4371-8dad-fccd8d9c4494");
        }
    }

    public class RoundtripTests
    {
        [Fact]

        public async Task RoundtripDTO()
        {
            using var dataStore = new DataFac.Storage.Testing.TestDataStore();

            var orig = new T_NameSpace_.MemBlocks.T_EntityName_();
            orig.BaseField1 = 789;
            orig.T_ScalarMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.T_RequiredEntityMemberName_ = new T_MemberTypeNameSpace_.MemBlocks.T_MemberTypeName_() { Field1 = 123 };
            await orig.Pack(dataStore);
            orig.Freeze();

            var buffer = orig.GetBuffer();

            var copy = T_NameSpace_.MemBlocks.T_EntityName_.CreateFrom(buffer);
            copy.IsFrozen.ShouldBeTrue();
            await copy.Unpack(dataStore, 0);
            copy.BaseField1.ShouldBe(orig.BaseField1);
            copy.T_ScalarMemberName_.ShouldBe(orig.T_ScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).ShouldBeTrue();
            copy.T_RequiredEntityMemberName_.ShouldNotBeNull();
            await copy.T_RequiredEntityMemberName_.Unpack(dataStore, 0);
            copy.T_RequiredEntityMemberName_!.Field1.ShouldBe(orig.T_RequiredEntityMemberName_.Field1);
        }
    }
}