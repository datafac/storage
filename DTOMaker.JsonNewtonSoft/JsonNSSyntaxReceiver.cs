using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DTOMaker.JsonNewtonSoft
{
    internal class JsonNSSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new JsonNSDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string nameSpace, string name, Location location) => new JsonNSEntity(domain, nameSpace, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new JsonNSMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            // not needed yet
        }

        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes)
        {
            // not needed yet
        }

        public JsonNSSyntaxReceiver() : base(DomainFactory, EntityFactory, MemberFactory)
        {
        }
    }
}
