using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.CSPoco
{
    internal class CSPocoSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnOnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxReceiverHelper.ProcessNode(context, Domains,
                (n, l) => new CSPocoDomain(n, l),
                (d, n, l) => new CSPocoEntity(d, n, l),
                (e, n, l) => new CSPocoMember(e, n, l));
        }
    }
}
