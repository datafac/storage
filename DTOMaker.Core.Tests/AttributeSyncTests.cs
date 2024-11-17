using FluentAssertions;

namespace DTOMaker.Runtime.Tests
{
    public class AttributeSyncTests
    {
        private void LayoutMethodsAreEquivalent(DTOMaker.Models.LayoutMethod a, DTOMaker.Gentime.LayoutMethod b)
        {
            int aOrdinal = (int)a;
            int bOrdinal = (int)b;
            bOrdinal.Should().Be(aOrdinal);
        }

        [Fact]
        public void LayoutMethodsMatch()
        {
            LayoutMethodsAreEquivalent(DTOMaker.Models.LayoutMethod.Undefined, DTOMaker.Gentime.LayoutMethod.Undefined);
            LayoutMethodsAreEquivalent(DTOMaker.Models.LayoutMethod.Explicit, DTOMaker.Gentime.LayoutMethod.Explicit);
            LayoutMethodsAreEquivalent(DTOMaker.Models.LayoutMethod.SequentialV1, DTOMaker.Gentime.LayoutMethod.SequentialV1);
        }

        [Fact]
        public void AttributeNamesMatch()
        {
            nameof(DTOMaker.Gentime.DomainAttribute).Should().Be(nameof(DTOMaker.Models.DomainAttribute));
            nameof(DTOMaker.Gentime.EntityAttribute).Should().Be(nameof(DTOMaker.Models.EntityAttribute));
            nameof(DTOMaker.Gentime.EntityLayoutAttribute).Should().Be(nameof(DTOMaker.Models.EntityLayoutAttribute));
            nameof(DTOMaker.Gentime.MemberAttribute).Should().Be(nameof(DTOMaker.Models.MemberAttribute));
            nameof(DTOMaker.Gentime.MemberLayoutAttribute).Should().Be(nameof(DTOMaker.Models.MemberLayoutAttribute));
        }
    }
}
