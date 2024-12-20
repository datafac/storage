using FluentAssertions;
using MessagePack;
using Sandpit.MessagePack;
using System;
using Xunit;

namespace Sandpit.Tests
{
    public class PolymorphicMessagePackTests
    {
        [Fact]
        public void Roundtrip01AsEntity()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<Rectangle>(orig);
            var copy = MessagePackSerializer.Deserialize<Rectangle>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            copy.Freeze();
            copy.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
        }

        [Fact]
        public void Roundtrip02AsBase()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<EntityBase>(orig);
            var recd = MessagePackSerializer.Deserialize<EntityBase>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            recd.Should().NotBeNull();
            recd.Should().BeOfType<Rectangle>();
            recd.Freeze();
            var copy = recd as Rectangle;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
        }

        [Fact]
        public void Roundtrip03AsParent()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<Quadrilateral>(orig);
            var recd = MessagePackSerializer.Deserialize<Quadrilateral>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);

            recd.Should().NotBeNull();
            recd.Should().BeOfType<Rectangle>();
            recd.Freeze();
            var copy = recd as Rectangle;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
        }
    }
}