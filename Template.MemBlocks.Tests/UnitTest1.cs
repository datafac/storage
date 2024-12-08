using FluentAssertions;
using System;

namespace Template.MemBlocks.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var orig = new T_DomainName_.MemBlocks.T_EntityName_();
            orig.T_ScalarMemberName_ = 123;
            orig.T_VectorMemberName_ = new int[] { 1, 2, 3 };
            orig.Freeze();

            var buffer = orig.Block;

            var copy = new T_DomainName_.MemBlocks.T_EntityName_(buffer);
            copy.IsFrozen().Should().BeTrue();
            copy.T_ScalarMemberName_.Should().Be(orig.T_ScalarMemberName_);
            copy.T_VectorMemberName_.Span.SequenceEqual(orig.T_VectorMemberName_.Span).Should().BeTrue();
        }
    }
}