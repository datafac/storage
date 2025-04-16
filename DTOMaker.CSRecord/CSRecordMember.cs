using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSRecord
{
    internal sealed class CSRecordMember : TargetMember
    {
        public CSRecordMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public CSRecordMember(TargetEntity entity, CSRecordMember source) : base(entity, source) { }
    }
}
