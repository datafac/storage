using DataFac.Memory;
using DTOMaker.Runtime.MessagePack;
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

            var buffer = sender.SerializeToMessagePack<MyOrg.Models.MessagePack.MyTree>();

            var recver = buffer.DeserializeFromMessagePack<MyOrg.Models.MessagePack.MyTree>();
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