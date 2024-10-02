using DTOMaker.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DTOMaker.Gentime
{
    public static class SyntaxReceiverHelper
    {
        // todo refactor and cleanup
        private static (string? error, T result) TryGetValue<T>(object? input, T defaultValue) where T : struct
        {
            if (input is null)
                return ($"Could not parse (null) as <{typeof(T).Name}>", defaultValue);
            else if (input is T value)
                return (null, value);
            else
                return ($"Could not parse '{input}' <{input.GetType().Name}> as <{typeof(T).Name}>", defaultValue);
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
                        // todo other entity details such as uniqueid
                    }
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityLayoutAttr)
                    {
                        // found entity layout details
                        var attributeArguments = entityLayoutAttr.ConstructorArguments;
                        if (attributeArguments.Length == 2)
                        {
                            (string? errorMessage, LayoutMethod method) = TryGetValue<LayoutMethod>(attributeArguments[0].Value, LayoutMethod.Undefined);
                            if (errorMessage is null)
                            {
                                entity.LayoutMethod = method;
                            }
                            else
                            {
                                entity.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, idsLocation, DiagnosticSeverity.Error,
                                        errorMessage));
                            }
                            (errorMessage, int blockLength) = TryGetValue<int>(attributeArguments[1].Value, 0);
                            if (errorMessage is null)
                            {
                                entity.BlockSize = blockLength;
                            }
                            else
                            {
                                entity.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, idsLocation, DiagnosticSeverity.Error,
                                        errorMessage));
                            }
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
                            var attributeArguments = memberAttr.ConstructorArguments;
                            if (attributeArguments.Length == 1)
                            {
                                member.MemberType = pdsSymbol.Type.Name;
                                (string? errorMessage, int sequence) = TryGetValue<int>(attributeArguments[0].Value, 0);
                                if (errorMessage is null)
                                {
                                    member.Sequence = sequence;
                                }
                                else
                                {
                                    entity.SyntaxErrors.Add(
                                        new SyntaxDiagnostic(
                                            DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
                                            errorMessage));
                                }
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
                            var attributeArguments = memberLayoutAttr.ConstructorArguments;
                            if (attributeArguments.Length == 3)
                            {
                                (string? errorMessage, int fieldOffset) = TryGetValue<int>(attributeArguments[0].Value, 0);
                                if (errorMessage is null)
                                {
                                    member.FieldOffset = fieldOffset;
                                }
                                else
                                {
                                    entity.SyntaxErrors.Add(
                                        new SyntaxDiagnostic(
                                            DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
                                            errorMessage));
                                }
                                (errorMessage, int fieldLength) = TryGetValue<int>(attributeArguments[1].Value, 0);
                                if (errorMessage is null)
                                {
                                    member.FieldLength = fieldLength;
                                }
                                else
                                {
                                    entity.SyntaxErrors.Add(
                                        new SyntaxDiagnostic(
                                            DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
                                            errorMessage));
                                }
                                (errorMessage, bool isBigEndian) = TryGetValue<bool>(attributeArguments[2].Value, false);
                                if (errorMessage is null)
                                {
                                    member.IsBigEndian = isBigEndian;
                                }
                                else
                                {
                                    entity.SyntaxErrors.Add(
                                        new SyntaxDiagnostic(
                                            DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, pdsLocation, DiagnosticSeverity.Error,
                                            errorMessage));
                                }
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
