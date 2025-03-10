using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSRecord
{
    internal sealed class CSRecordEntity : TargetEntity
    {
        public CSRecordEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(domain, nameSpace, name, location) { }
    }
}
