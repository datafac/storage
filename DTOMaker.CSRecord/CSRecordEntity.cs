using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSRecord
{
    internal sealed class CSRecordEntity : TargetEntity
    {
        public CSRecordEntity(TargetDomain domain, TypeFullName entityName, Location location) 
            : base(domain, entityName, location) { }
    }
}
