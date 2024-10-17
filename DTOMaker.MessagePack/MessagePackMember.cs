using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackMember : TargetMember
    {
        public MessagePackMember(string name, Location location) : base(name, location) { }
    }
}
