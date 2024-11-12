using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace DTOMaker.CSPoco
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public ConcurrentDictionary<string, TargetDomain> Domains { get; } = new ConcurrentDictionary<string, TargetDomain>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxReceiverHelper.ProcessNode(context, Domains,
                (n, l) => new CSPocoDomain(n, l),
                (n, l) => new CSPocoEntity(n, l),
                (n, l) => new CSPocoMember(n, l));
        }
    }
}
