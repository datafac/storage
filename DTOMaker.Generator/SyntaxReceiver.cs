using DTOMaker.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DTOMaker.Generator
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        private static T TryGetValue<T>(object? input, T defaultValue) => input is T value ? value : defaultValue;

        public ConcurrentDictionary<string, TargetDomain> Domains { get; } = new ConcurrentDictionary<string, TargetDomain>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InterfaceDeclarationSyntax ids1
                && ids1.Modifiers.Any(SyntaxKind.PublicKeyword)
                && ids1.Parent is NamespaceDeclarationSyntax nds1
                && ids1.AttributeLists.Count > 0
                && ids1.HasOneAttributeNamed(nameof(EntityAttribute)))
            {
                Location ndsLocation = Location.Create(nds1.SyntaxTree, nds1.Span);
                Location idsLocation = Location.Create(ids1.SyntaxTree, ids1.Span);
                var domain = Domains.GetOrAdd(nds1.Name.ToString(), (n) => new TargetDomain(n, ndsLocation));
                string interfaceName = ids1.Identifier.Text;
                if (interfaceName.Length <= 1 || !interfaceName.StartsWith("I"))
                {
                    domain.SyntaxErrors.Add(
                        new SyntaxDiagnostic(idsLocation, DiagnosticSeverity.Error,
                            $"Expected interface named '{interfaceName}' to start with 'I'."));
                }
                else
                {
                    string entityName = interfaceName.Substring(1);
                    var entity = domain.Entities.GetOrAdd(entityName, (n) => new TargetEntity(n, idsLocation));
                    if (context.SemanticModel.GetDeclaredSymbol(ids1) is not INamedTypeSymbol symbol)
                    {
                        entity.SyntaxErrors.Add(new SyntaxDiagnostic(idsLocation, DiagnosticSeverity.Error,
                            $"Cannot get symbol from semantic model for: {ids1.Identifier}"));
                    }
                    else
                    {
                        var attributes = symbol.GetAttributes();
                        var attribute = attributes[0];

                        var attributeArguments = attribute.ConstructorArguments;
                        if (attributeArguments.Length == 1)
                        {
                            entity.BlockSize = TryGetValue<int>(attributeArguments[0].Value, 0);
                        }
                        else
                        {
                            entity.SyntaxErrors.Add(
                                new SyntaxDiagnostic(idsLocation, DiagnosticSeverity.Error,
                                    $"Expected {nameof(EntityAttribute)} attribute to have 1 argument, but it has {attributeArguments.Length}."));
                        }
                    }
                }
            }

            if (context.Node is PropertyDeclarationSyntax pds2
                && pds2.Parent is InterfaceDeclarationSyntax ids2
                && ids2.Parent is NamespaceDeclarationSyntax nds2
                && pds2.AttributeLists.Count > 0
                && pds2.HasOneAttributeNamed(nameof(MemberAttribute)))
            {
                Location pdsLocation = Location.Create(pds2.SyntaxTree, pds2.Span);
                string domainName = nds2.Name.ToString();
                string interfaceName = ids2.Identifier.Text;
                string entityName = interfaceName.Substring(1);
                if (Domains.TryGetValue(domainName, out var domain)
                    && domain.Entities.TryGetValue(entityName, out var entity))
                {
                    var member = entity.Members.GetOrAdd(pds2.Identifier.Text, (n) => new TargetMember(n, pdsLocation));
                    var symbol = context.SemanticModel.GetDeclaredSymbol(pds2);
                    if (symbol is null)
                    {
                        member.SyntaxErrors.Add(new SyntaxDiagnostic(pdsLocation, DiagnosticSeverity.Error,
                            $"Cannot get symbol from semantic model for: {pds2.Identifier}"));
                    }
                    else
                    {
                        var attributes = symbol.GetAttributes();
                        var attribute = attributes[0];

                        var attributeArguments = attribute.ConstructorArguments;
                        if (attributeArguments.Length == 2)
                        {
                            int offset = TryGetValue<int>(attributeArguments[0].Value, 0);
                            int length = TryGetValue<int>(attributeArguments[1].Value, 0);
                            member.MemberType = symbol.Type.Name;
                            member.FieldOffset = offset;
                            member.FieldLength = length;
                        }
                        // else if todo 3 arg ctor includes isBigEndian arg
                        else
                        {
                            member.SyntaxErrors.Add(new SyntaxDiagnostic(pdsLocation, DiagnosticSeverity.Error,
                                $"Expected {nameof(MemberAttribute)} attribute to have 1 argument, but it has {attributeArguments.Length}"));
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
