using DTOMaker.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public static class SyntaxReceiverHelper
    {
        private static (string? error, T result) TryGetValue<T>(object? input, T defaultValue) where T : struct
        {
            if (input is null)
                return ($"Could not parse (null) as <{typeof(T).Name}>", defaultValue);
            else if (input is T value)
                return (null, value);
            else
                return ($"Could not parse '{input}' <{input.GetType().Name}> as <{typeof(T).Name}>", defaultValue);
        }

        private static void TryGetAttributeArgumentValue<T>(TargetBase target, Location location, ImmutableArray<TypedConstant> attributeArguments, int index, Action<T> action)
            where T : struct
        {
            (string? errorMessage, T value) = TryGetValue<T>(attributeArguments[0].Value, default);
            if (errorMessage is null)
            {
                action(value);
            }
            else
            {
                target.SyntaxErrors.Add(
                    new SyntaxDiagnostic(
                        DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                        errorMessage));
            }
        }

        public static void ProcessNode(GeneratorSyntaxContext context, ConcurrentDictionary<string, TargetDomain> domains,
            Func<string, Location, TargetDomain> domainFactory,
            Func<string, Location, TargetEntity> entityFactory,
            Func<string, Location, TargetMember> memberFactory)
        {
            if (context.Node is InterfaceDeclarationSyntax ids
                && ids.Modifiers.Any(SyntaxKind.PublicKeyword)
                && context.SemanticModel.GetDeclaredSymbol(ids) is INamedTypeSymbol idsSymbol)
            {
                if (ids.Parent is NamespaceDeclarationSyntax nds && ids.AttributeLists.Count > 0)
                {
                    Location ndsLocation = Location.Create(nds.SyntaxTree, nds.Span);
                    Location idsLocation = Location.Create(ids.SyntaxTree, ids.Span);
                    var domain = domains.GetOrAdd(nds.Name.ToString(), (n) => domainFactory(n, ndsLocation));
                    string interfaceName = ids.Identifier.Text;
                    if (interfaceName.Length <= 1 || !interfaceName.StartsWith("I"))
                    {
                        domain.SyntaxErrors.Add(
                            new SyntaxDiagnostic(
                                DiagnosticId.DTOM0001, "Invalid interface name", DiagnosticCategory.Naming, idsLocation, DiagnosticSeverity.Error,
                                $"Expected interface named '{interfaceName}' to start with 'I'."));
                    }
                    string entityName = interfaceName.Substring(1);
                    var entity = domain.Entities.GetOrAdd(entityName, (n) => entityFactory(n, idsLocation));
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityAttribute)) is AttributeData entityAttr)
                    {
                        // found opt-in entity
                        entity.HasEntityAttribute = true;
                        // todo other entity details such as uniqueid
                    }
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityLayoutAttr)
                    {
                        // found entity layout details
                        entity.HasEntityLayoutAttribute = true;
                        var attributeArguments = entityLayoutAttr.ConstructorArguments;
                        if (attributeArguments.Length == 2)
                        {
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 0, (value) => { entity.LayoutMethod = (LayoutMethod)value; });
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 1, (value) => { entity.BlockLength = value; });
                        }
                        else
                        {
                            entity.SyntaxErrors.Add(
                                new SyntaxDiagnostic(
                                    DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, idsLocation, DiagnosticSeverity.Error,
                                    $"Expected {nameof(EntityLayoutAttribute)} attribute to have 2 arguments, but it has {attributeArguments.Length}."));
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
                        var member = entity.Members.GetOrAdd(pds.Identifier.Text, (n) => memberFactory(n, pdsLocation));
                        if (pdsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberAttribute)) is AttributeData memberAttr)
                        {
                            member.HasMemberAttribute = true;
                            member.MemberType = pdsSymbol.Type.Name;
                            var attributeArguments = memberAttr.ConstructorArguments;
                            if (attributeArguments.Length == 1)
                            {
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 0, (value) => { member.Sequence = value; });
                            }
                            else
                            {
                                member.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
                                        $"Expected {nameof(MemberAttribute)} attribute to have 1 argument, but it has {attributeArguments.Length}"));
                            }
                        }
                        if (pdsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberLayoutAttribute)) is AttributeData memberLayoutAttr)
                        {
                            member.HasMemberLayoutAttribute = true;
                            member.MemberType = pdsSymbol.Type.Name;
                            var attributeArguments = memberLayoutAttr.ConstructorArguments;
                            if (attributeArguments.Length == 3)
                            {
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 0, (value) => { member.FieldOffset = value; });
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 1, (value) => { member.FieldLength = value; });
                                TryGetAttributeArgumentValue<bool>(member, pdsLocation, attributeArguments, 2, (value) => { member.IsBigEndian = value; });
                            }
                            else
                            {
                                member.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
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
