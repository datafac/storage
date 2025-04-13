using Shouldly;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Template.CSRecord.Tests
{
#if NET8_0_OR_GREATER
    public class EqualityTests
    {
        [Fact]
        public void Equality01_HashCodes()
        {
            var orig = new T_NameSpace_.CSRecord.T_EntityImplName_();
            int origHash = orig.GetHashCode();

            var copy = orig with { T_RequiredScalarMemberName_ = 0 };

            int copyHash = copy.GetHashCode();
            copyHash.ShouldBe(origHash);
        }

        [Fact]
        public void Equality02_ScalarData()
        {
            var orig = new T_NameSpace_.CSRecord.T_EntityImplName_() { T_RequiredScalarMemberName_ = 123 };
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSRecord.T_EntityImplName_() { T_RequiredScalarMemberName_ = 123 };
            int copyHash = copy.GetHashCode();

            copyHash.ShouldBe(origHash);
            copy.ShouldBe(orig);
            copy.Equals(orig).ShouldBeTrue();
        }

        [Fact]
        public void Equality03_VectorData()
        {
            var orig = new T_NameSpace_.CSRecord.T_EntityImplName_() { T_VectorMemberName_ = new int[] { 123, 456, 789 } };
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSRecord.T_EntityImplName_() { T_VectorMemberName_ = new int[] { 123, 456, 789 } };
            int copyHash = copy.GetHashCode();

            copyHash.ShouldBe(origHash);
            copy.ShouldBe(orig);
            copy.Equals(orig).ShouldBeTrue();
        }
    }
#endif
}