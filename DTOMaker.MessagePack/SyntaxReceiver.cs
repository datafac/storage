using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace DTOMaker.MessagePack
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public ConcurrentDictionary<string, TargetDomain> Domains { get; } = new ConcurrentDictionary<string, TargetDomain>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxReceiverHelper.ProcessNode(context, Domains,
                (n, l) => new MessagePackDomain(n, l),
                (d, n, l) => new MessagePackEntity(d, n, l),
                (e, n, l) => new MessagePackMember(e, n, l));
        }
    }
}
