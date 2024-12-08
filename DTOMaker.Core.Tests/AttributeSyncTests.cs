using FluentAssertions;

namespace DTOMaker.Runtime.Tests
{
    public class AttributeSyncTests
    {
        [Fact]
        public void AttributeNamesMatch()
        {
            nameof(DTOMaker.Gentime.DomainAttribute).Should().Be(nameof(DTOMaker.Models.DomainAttribute));
            nameof(DTOMaker.Gentime.EntityAttribute).Should().Be(nameof(DTOMaker.Models.EntityAttribute));
            nameof(DTOMaker.Gentime.MemberAttribute).Should().Be(nameof(DTOMaker.Models.MemberAttribute));
        }
    }
}
