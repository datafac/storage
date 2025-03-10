using Shouldly;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Template.CSRecord.Tests
{
    public class EqualityTests
    {
        [Fact]
        public void Equality01_HashCodes()
        {
            var sut = new T_NameSpace_.CSRecord.T_EntityName_();
            int hc0 = sut.GetHashCode();

            int hc1 = sut.GetHashCode();
            hc1.ShouldBe(hc0);
        }

        [Fact]
        public void Equality02_ScalarData()
        {
            var orig = new T_NameSpace_.CSRecord.T_EntityName_() { T_RequiredScalarMemberName_ = 123 };
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSRecord.T_EntityName_() { T_RequiredScalarMemberName_ = 123 };
            int copyHash = copy.GetHashCode();

            copyHash.ShouldBe(origHash);
            copy.ShouldBe(orig);
            copy.Equals(orig).ShouldBeTrue();
        }

        [Fact] //(Skip = "todo we need an array-like structure that implements IEquatable")]
        public void Equality03_VectorData()
        {
            var orig = new T_NameSpace_.CSRecord.T_EntityName_() { T_VectorMemberName_ = new int[] { 123, 456, 789 } };
            int origHash = orig.GetHashCode();

            var copy = new T_NameSpace_.CSRecord.T_EntityName_() { T_VectorMemberName_ = new int[] { 123, 456, 789 } };
            int copyHash = copy.GetHashCode();

            copyHash.ShouldBe(origHash);
            copy.ShouldBe(orig);
            copy.Equals(orig).ShouldBeTrue();
        }
    }
}