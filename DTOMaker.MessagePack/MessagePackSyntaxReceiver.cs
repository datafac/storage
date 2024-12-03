using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.MessagePack
{
    internal class MessagePackSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnOnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxReceiverHelper.ProcessNode(context, Domains,
                (n, l) => new MessagePackDomain(n, l),
                (d, n, l) => new MessagePackEntity(d, n, l),
                (e, n, l) => new MessagePackMember(e, n, l));
        }
    }
}
