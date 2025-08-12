using DataFac.Memory;
using DTOMaker.Runtime.JsonNewtonSoft;
using DTOMaker.Runtime.MessagePack;
using MessagePack;
using Newtonsoft.Json;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Template.JsonNewtonSoft.Tests
{
    internal interface ISimple
    {
        int Field1 { get; }
        Octets Field2 { get; }
    }

    [MessagePackObject]
    public sealed class SimpleMP : ISimple
    {
        [Key(1)]
        public int Field1 { get; set; }

        [Key(2)]
        public ReadOnlyMemory<byte> Field2 { get; set; }

        Octets ISimple.Field2 => Octets.UnsafeWrap(Field2);
    }

    internal sealed class SimpleNS : ISimple
    {
        [JsonProperty("fieldOne")]
        public int Field1 { get; set; }

        [JsonProperty("fieldTwo")]
        public byte[] Field2 { get; set; } = Array.Empty<byte>();

        Octets ISimple.Field2 => Octets.UnsafeWrap(Field2);
    }

    internal interface IParent
    {
        int Id { get; }
    }

    [MessagePackObject]
    [Union(1, typeof(Child1MP))]
    public abstract class ParentMP : IParent
    {
        [Key(101)]
        public int Id { get; set; }
    }

    internal class ParentNS : IParent
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    internal interface IChild1 : IParent
    {
        string Name { get; }
    }

    [MessagePackObject]
    public sealed class Child1MP : ParentMP, IChild1
    {
        [Key(201)]
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class Child1NS : ParentNS, IChild1
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class SandboxTests
    {
        [Fact]
        public void RoundtripSimpleMP()
        {
            ReadOnlyMemory<byte> smallBinary = new byte[] { 1, 2, 3, 4, 5, 6, 7 };

            var orig = new SimpleMP();
            orig.Field1 = 321;
            orig.Field2 = smallBinary;

            ReadOnlyMemory<byte> buffer = orig.SerializeToMessagePack<SimpleMP>();
            var copy = buffer.DeserializeFromMessagePack<SimpleMP>();

            ISimple iorig = orig;
            ISimple icopy = copy;
            icopy.Field1.ShouldBe(iorig.Field1);
            icopy.Field2.AsMemory().Span.SequenceEqual(iorig.Field2.AsMemory().Span).ShouldBeTrue();
        }

        [Fact]
        public void RoundtripSimpleNS()
        {
            ReadOnlyMemory<byte> smallBinary = new byte[] { 1, 2, 3, 4, 5, 6, 7 };

            var orig = new SimpleNS();
            orig.Field1 = 321;
            orig.Field2 = smallBinary.ToArray();

            string buffer = orig.SerializeToJson<SimpleNS>();
            var copy = buffer.DeserializeFromJson<SimpleNS>();

            copy.ShouldNotBeNull();

            ISimple iorig = orig;
            ISimple icopy = copy;
            icopy.Field1.ShouldBe(iorig.Field1);
            icopy.Field2.AsMemory().Span.SequenceEqual(iorig.Field2.AsMemory().Span).ShouldBeTrue();
        }

        [Fact]
        public void RoundtripNestedMPAsLeaf()
        {
            var orig = new Child1MP();
            orig.Id = 321;
            orig.Name = "Alice";

            ReadOnlyMemory<byte> buffer = orig.SerializeToMessagePack<Child1MP>();
            var copy = buffer.DeserializeFromMessagePack<Child1MP>();

            IChild1 iorig = orig;
            IChild1 icopy = copy;
            icopy.Id.ShouldBe(iorig.Id);
            icopy.Name.ShouldBe(iorig.Name);
        }

        [Fact]
        public void RoundtripNestedMPAsRoot()
        {
            var orig = new Child1MP();
            orig.Id = 321;
            orig.Name = "Alice";

            ReadOnlyMemory<byte> buffer = orig.SerializeToMessagePack<ParentMP>();
            var copy = buffer.DeserializeFromMessagePack<ParentMP>();

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<Child1MP>();

            IChild1 iorig = orig;
            IChild1? icopy = (copy as IChild1);
            icopy.ShouldNotBeNull();
            icopy.Id.ShouldBe(iorig.Id);
            icopy.Name.ShouldBe(iorig.Name);
        }

        [Fact]
        public void RoundtripNestedNSAsLeaf()
        {
            var orig = new Child1NS();
            orig.Id = 321;
            orig.Name = "Alice";

            string buffer = orig.SerializeToJson<Child1NS>();
            var copy = buffer.DeserializeFromJson<Child1NS>();

            copy.ShouldNotBeNull();

            IChild1 iorig = orig;
            IChild1 icopy = copy;
            icopy.Id.ShouldBe(iorig.Id);
            icopy.Name.ShouldBe(iorig.Name);
        }

        [Fact]
        public void RoundtripNestedNSAsRoot()
        {
            var orig = new Child1NS();
            orig.Id = 321;
            orig.Name = "Alice";

            string buffer = orig.SerializeToJson<ParentNS>();
            var copy = buffer.DeserializeFromJson<ParentNS>();

            copy.ShouldNotBeNull();
            copy.ShouldBeOfType<Child1NS>();

            IChild1 iorig = orig;
            IChild1? icopy = (copy as IChild1);
            icopy.ShouldNotBeNull();
            icopy.Id.ShouldBe(iorig.Id);
            icopy.Name.ShouldBe(iorig.Name);
        }
    }
}