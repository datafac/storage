using FluentAssertions;
using Xunit;

namespace Template_CSPoco.Tests
{
    public class EqualityTests
    {
        [Fact]
        public void Equality01_HashCodes()
        {
            var sut = new T_NameSpace_.CSPoco.T_EntityName_();
            int hc0 = sut.GetHashCode();

            sut.Freeze();
            int hc1 = sut.GetHashCode();
            hc1.Should().Be(hc0);

        }

        [Fact]
        public void Equality02_ScalarData()
        {
            var orig = new T_NameSpace_.CSPoco.T_EntityName_() { T_ScalarRequiredMemberName_ = 123 };
            orig.Freeze();
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSPoco.T_EntityName_() { T_ScalarRequiredMemberName_ = 123 };
            copy.Freeze();
            int copyHash = copy.GetHashCode();

            copyHash.Should().Be(origHash);
            copy.Should().Be(orig);
            copy.Equals(orig).Should().BeTrue();
        }

        [Fact]
        public void Equality03_VectorData()
        {
            var orig = new T_NameSpace_.CSPoco.T_EntityName_() { T_VectorMemberName_ = new int[] { 123, 456, 789} };
            orig.Freeze();
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSPoco.T_EntityName_() { T_VectorMemberName_ = new int[] { 123, 456, 789 } };
            copy.Freeze();
            int copyHash = copy.GetHashCode();

            copyHash.Should().Be(origHash);
            copy.Should().Be(orig);
            copy.Equals(orig).Should().BeTrue();
        }
    }
}