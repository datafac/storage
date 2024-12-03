using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackMember : TargetMember
    {
        public MessagePackMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
    }
}
