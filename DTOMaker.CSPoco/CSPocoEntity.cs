using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSPoco
{
    internal sealed class CSPocoEntity : TargetEntity
    {
        public CSPocoEntity(TargetDomain domain, TypeFullName entityName, Location location) 
            : base(domain, entityName, location) { }
    }
}
