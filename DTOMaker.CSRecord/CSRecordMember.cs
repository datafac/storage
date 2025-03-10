using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSRecord
{
    internal sealed class CSRecordMember : TargetMember
    {
        public CSRecordMember(TargetEntity entity, string name, Location location) : base(entity,  name, location) { }
    }
}
