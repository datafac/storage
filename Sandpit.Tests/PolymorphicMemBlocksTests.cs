using FluentAssertions;
using Sandpit.MemBlocks;
using Xunit;

namespace Sandpit.Tests
{
    public class PolymorphicMemBlocksTests
    {
        [Fact]
        public void Roundtrip01AsEntity()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            var buffers = orig.GetBuffers();
            var copy = new Rectangle(buffers);

            copy.Freeze();
            copy.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
            copy.Should().Be(orig);
            copy.Equals(orig).Should().BeTrue();
        }

        [Fact]
        public void Roundtrip02AsBase()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            var buffers = ((EntityBase)orig).GetBuffers();
            string entityId = orig.GetEntityId();
            EntityBase recd = EntityBase.CreateFrom(entityId, buffers);

            recd.Should().NotBeNull();
            recd.Should().BeOfType<Rectangle>();
            recd.Freeze();
            var copy = recd as Rectangle;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
            copy.Should().Be(orig);
            copy.Equals(orig).Should().BeTrue();
        }

        [Fact]
        public void Roundtrip03AsParent()
        {
            var orig = new Rectangle();
            orig.Length = 3.0D;
            orig.Height = 2.0D;
            orig.Freeze();

            var buffers = ((Quadrilateral)orig).GetBuffers();
            string entityId = orig.GetEntityId();
            Quadrilateral recd = Quadrilateral.CreateFrom(entityId, buffers);

            recd.Should().NotBeNull();
            recd.Should().BeOfType<Rectangle>();
            recd.Freeze();
            var copy = recd as Rectangle;
            copy.Should().NotBeNull();
            copy!.IsFrozen.Should().BeTrue();
            copy.Length!.Should().Be(orig.Length);
            copy.Height.Should().Be(orig.Height);
            copy.Should().Be(orig);
            copy.Equals(orig).Should().BeTrue();
        }
    }
}