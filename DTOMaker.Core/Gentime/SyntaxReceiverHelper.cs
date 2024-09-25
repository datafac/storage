using DTOMaker.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Linq;

namespace DTOMaker.Gentime
{
    public static class SyntaxReceiverHelper
    {
        private static T TryGetValue<T>(object? input, T defaultValue) => input is T value ? value : defaultValue;

        public static void ProcessNode(GeneratorSyntaxContext context, ConcurrentDictionary<string, TargetDomain> domains)
        {
            if (context.Node is InterfaceDeclarationSyntax ids
                && ids.Modifiers.Any(SyntaxKind.PublicKeyword)
                && context.SemanticModel.GetDeclaredSymbol(ids) is INamedTypeSymbol idsSymbol)
            {
                if (ids.Parent is NamespaceDeclarationSyntax nds && ids.AttributeLists.Count > 0)
                {
                    Location ndsLocation = Location.Create(nds.SyntaxTree, nds.Span);
                    Location idsLocation = Location.Create(ids.SyntaxTree, ids.Span);
                    var domain = domains.GetOrAdd(nds.Name.ToString(), (n) => new TargetDomain(n, ndsLocation));
                    string interfaceName = ids.Identifier.Text;
                    if (interfaceName.Length <= 1 || !interfaceName.StartsWith("I"))
                    {
                        domain.SyntaxErrors.Add(
                            new SyntaxDiagnostic(idsLocation, DiagnosticSeverity.Error,
                                $"Expected interface named '{interfaceName}' to start with 'I'."));
                    }
                    string entityName = interfaceName.Substring(1);
                    var entity = domain.Entities.GetOrAdd(entityName, (n) => new TargetEntity(n, idsLocation));
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityAttribute)) is AttributeData entityAttr)
                    {
                        // found opt-in entity
                        // todo other entity details such as uniqueid
                    }
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityLayoutAttr)
                    {
                        // found entity layout details
                        var attributeArguments = entityLayoutAttr.ConstructorArguments;
                        if (attributeArguments.Length == 1)
                        {
                            entity.BlockSize = TryGetValue<int>(attributeArguments[0].Value, 0);
                        }
                        else
                        {
                            entity.SyntaxErrors.Add(
                                new SyntaxDiagnostic(idsLocation, DiagnosticSeverity.Error,
                                    $"Expected {nameof(EntityLayoutAttribute)} attribute to have 1 argument, but it has {attributeArguments.Length}."));
                        }
                    }
                }
            }

            if (context.Node is PropertyDeclarationSyntax pds
                && context.SemanticModel.GetDeclaredSymbol(pds) is IPropertySymbol pdsSymbol)
            {
                if (pds.Parent is InterfaceDeclarationSyntax ids2
                    && ids2.Parent is NamespaceDeclarationSyntax nds2
                    && pds.AttributeLists.Count > 0)
                {
                    string domainName = nds2.Name.ToString();
                    string interfaceName = ids2.Identifier.Text;
                    string entityName = interfaceName.Substring(1);
                    if (domains.TryGetValue(domainName, out var domain)
                        && domain.Entities.TryGetValue(entityName, out var entity))
                    {
                        Location pdsLocation = Location.Create(pds.SyntaxTree, pds.Span);
                        var member = entity.Members.GetOrAdd(pds.Identifier.Text, (n) => new TargetMember(n, pdsLocation));
                        if (pdsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberAttribute)) is AttributeData memberAttr)
                        {
                            var attributeArguments = memberAttr.ConstructorArguments;
                            if (attributeArguments.Length == 1)
                            {
                                member.Sequence = TryGetValue<int>(attributeArguments[0].Value, 0);
                            }
                            else
                            {
                                member.SyntaxErrors.Add(new SyntaxDiagnostic(pdsLocation, DiagnosticSeverity.Error,
                                    $"Expected {nameof(MemberAttribute)} attribute to have 1 argument, but it has {attributeArguments.Length}"));
                            }
                        }
                        if (pdsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberLayoutAttribute)) is AttributeData memberLayoutAttr)
                        {
                            var attributeArguments = memberLayoutAttr.ConstructorArguments;
                            if (attributeArguments.Length == 3)
                            {
                                member.MemberType = pdsSymbol.Type.Name;
                                member.FieldOffset = TryGetValue<int>(attributeArguments[0].Value, 0);
                                member.FieldLength = TryGetValue<int>(attributeArguments[1].Value, 0);
                                member.IsBigEndian = TryGetValue<bool>(attributeArguments[2].Value, false);
                            }
                            else
                            {
                                member.SyntaxErrors.Add(new SyntaxDiagnostic(pdsLocation, DiagnosticSeverity.Error,
                                    $"Expected {nameof(MemberLayoutAttribute)} attribute to have 3 arguments, but it has {attributeArguments.Length}"));
                            }
                        }
                    }
                    else
                    {
                        // ignore orphan member
                    }
                }
            }
        }
    }
}
