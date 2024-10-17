using FluentAssertions;
using MessagePack;
using System;
using System.Linq;

using T_Namespace_.MessagePack;

namespace Template_MessagePack.Tests
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(123L, "92-C0-7B")]
        public void Test1(long value, string expectedBytes)
        {
            var orig = new T_EntityName_();
            orig.T_MemberName_ = value;
            ReadOnlyMemory<byte> buffer = MessagePackSerializer.Serialize<T_EntityName_>(orig);

            string.Join("-", buffer.ToArray().Select(b => b.ToString("X2"))).Should().Be(expectedBytes);

            var copy = MessagePackSerializer.Deserialize<T_EntityName_>(buffer, out int bytesRead);

            bytesRead.Should().Be(buffer.Length);
            copy.T_MemberName_.Should().Be(orig.T_MemberName_);
        }
    }
}