using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DTOMaker.CSRecord
{
    internal class CSRecordSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes) { }
        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes) { }
        public CSRecordSyntaxReceiver() : base(new CSRecordFactory()) { }
    }
}
