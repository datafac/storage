using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace DTOMaker.MemBlocks
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public ConcurrentDictionary<string, TargetDomain> Domains { get; } = new ConcurrentDictionary<string, TargetDomain>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxReceiverHelper.ProcessNode(context, Domains,
                (n, l) => new MemBlockDomain(n, l),
                (n, l) => new MemBlockEntity(n, l),
                (n, l) => new MemBlockMember(n, l));
        }
    }
}
