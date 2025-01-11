using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSPoco
{
    internal sealed class CSPocoEntity : TargetEntity
    {
        public CSPocoEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(domain, nameSpace, name, location) { }
    }
}
