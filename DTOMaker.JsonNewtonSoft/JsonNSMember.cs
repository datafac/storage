using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonNewtonSoft
{
    internal sealed class JsonNSMember : TargetMember
    {
        public JsonNSMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
    }
}
