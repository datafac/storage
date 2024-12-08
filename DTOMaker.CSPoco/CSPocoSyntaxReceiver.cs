using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DTOMaker.CSPoco
{
    internal class CSPocoSyntaxReceiver : SyntaxReceiverBase
    {
        private static TargetDomain DomainFactory(string name, Location location) => new CSPocoDomain(name, location);
        private static TargetEntity EntityFactory(TargetDomain domain, string name, Location location) => new CSPocoEntity(domain, name, location);
        private static TargetMember MemberFactory(TargetEntity entity, string name, Location location) => new CSPocoMember(entity, name, location);

        protected override void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes)
        {
            // no additional processing required yet
        }

        public CSPocoSyntaxReceiver() : base(DomainFactory, EntityFactory, MemberFactory)
        {
        }
    }
}
