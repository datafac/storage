using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace DTOMaker.Gentime
{
    public abstract class SyntaxReceiverBase : ISyntaxContextReceiver
    {
        public ConcurrentDictionary<string, TargetDomain> Domains { get; } = new ConcurrentDictionary<string, TargetDomain>();

        protected abstract void OnOnVisitSyntaxNode(GeneratorSyntaxContext context);
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) => OnOnVisitSyntaxNode(context);
    }
}
