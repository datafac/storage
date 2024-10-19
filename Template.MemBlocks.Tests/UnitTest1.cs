using FluentAssertions;

namespace Template.MemBlocks.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var orig = new T_DomainName_.MemBlocks.T_EntityName_();
            orig.T_MemberName_ = System.DayOfWeek.Wednesday;
            orig.Freeze();

            var buffer = orig.Block;

            var copy = new T_DomainName_.MemBlocks.T_EntityName_(buffer);
            copy.IsFrozen().Should().BeTrue();
            copy.T_MemberName_.Should().Be(orig.T_MemberName_);
        }
    }
}