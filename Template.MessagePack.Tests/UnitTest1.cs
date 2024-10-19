using FluentAssertions;
using MessagePack;
using System;
using System.Linq;

using T_DomainName_.MessagePack;

namespace Template_MessagePack.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var orig = new T_EntityName_();
            orig.T_MemberName_ = DayOfWeek.Wednesday;
            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_EntityName_>(orig);
            var copy = MessagePackSerializer.Deserialize<T_EntityName_>(buffer, out int bytesRead);
            bytesRead.Should().Be(buffer.Length);
            copy.T_MemberName_.Should().Be(orig.T_MemberName_);
        }
    }
}