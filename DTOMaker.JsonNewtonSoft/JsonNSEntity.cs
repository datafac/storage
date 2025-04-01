using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonNewtonSoft
{
    internal sealed class JsonNSEntity : TargetEntity
    {
        public JsonNSEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(domain, nameSpace, name, location) { }
    }
}
