using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonNewtonSoft
{
    internal sealed class JsonNSEntity : TargetEntity
    {
        public JsonNSEntity(TargetDomain domain, TypeFullName entityName, Location location)
            : base(domain, entityName, location) { }
    }
}
