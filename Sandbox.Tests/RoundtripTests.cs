using DataFac.Memory;
using MessagePack;
using Shouldly;
using System;
using Xunit;

namespace Sandbox.Tests
{
    public class RoundtripTests
    {
        [Fact]
        public void Roundtrip_Octets_MessagePack()
        {
            var orig = new MyOrg.Models.MessagePack.MyTree()
            {
                Count = 1,
                Key = "abc",
                Value = Octets.Empty.AsMemory(),
                // todo recurse
            };
            orig.Freeze();

            var sender = new MyOrg.Models.MessagePack.MyTree(orig);
            sender.Freeze();

            var buffer = MessagePackSerializer.Serialize<MyOrg.Models.MessagePack.MyTree>(sender);

            var recver = MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.MyTree>(buffer);
            recver.Freeze();

            recver.Equals(sender).ShouldBeTrue();

            var copy = new MyOrg.Models.MessagePack.MyTree(recver);
            copy.Freeze();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Count == orig.Count).ShouldBeTrue();
            (copy.Key == orig.Key).ShouldBeTrue();
            copy.Value.Span.SequenceEqual(orig.Value.Span).ShouldBeTrue();
        }
    }
}