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
        private static void TryGetAttributeArgumentValue<T>(TargetBase target, Location location, ImmutableArray<TypedConstant> attributeArguments, int index, Action<T> action)
        {
            object? input = attributeArguments[index].Value;
            if (input is T value)
            {
                action(value);
                return;
            }

            string? errorMessage = input is null
                ? $"Could not parse arg[{index}] (null) as <{typeof(T).Name}>"
                : $"Could not parse arg[{index}] '{input}' <{input.GetType().Name}> as <{typeof(T).Name}>";

            target.SyntaxErrors.Add(
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    errorMessage));
        }

        private static bool CheckAttributeArguments(string attributeName, ImmutableArray<TypedConstant> arguments, int expectedCount, TargetBase target, Location location)
        {
            if (arguments.Length == expectedCount)
                return true;

            target.SyntaxErrors.Add(
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    $"Expected {attributeName} attribute to have {expectedCount} arguments, but it has {arguments.Length}."));
            return false;
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
                        // found entity attribute
                        entity.HasEntityAttribute = true;
                        // entity parent
                        entity.ParentName = "EntityBase";
                        if (idsSymbol.Interfaces.Length > 1)
                        {
                            // too many interfaces!
                            domain.SyntaxErrors.Add(
                                new SyntaxDiagnostic(
                                    DiagnosticId.DTOM0008, "Invalid interface parent(s)", DiagnosticCategory.Design, idsLocation, DiagnosticSeverity.Error,
                                    $"This interface can only be derived from one optional parent interface."));
                        }
                        else if (idsSymbol.Interfaces.Length == 1)
                        {
                            string parentName = idsSymbol.Interfaces[0].Name;
                            if (parentName.Length <= 1 || !parentName.StartsWith("I"))
                            {
                                domain.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0001, "Invalid interface name", DiagnosticCategory.Naming, idsLocation, DiagnosticSeverity.Error,
                                        $"Expected interface named '{parentName}' to start with 'I'."));
                            }
                            else
                            {
                                entity.ParentName = parentName.Substring(1);
                            }
                        }
                        var attributeArguments = entityAttr.ConstructorArguments;
                        if (CheckAttributeArguments(nameof(EntityAttribute), attributeArguments, 1, entity, idsLocation))
                        {
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 0, (value) => { entity.Tag = value; });
                        }
                    }
                    if (idsSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityLayoutAttribute)) is AttributeData entityLayoutAttr)
                    {
                        // found entity layout attribute
                        entity.HasEntityLayoutAttribute = true;
                        var attributeArguments = entityLayoutAttr.ConstructorArguments;
                        if (CheckAttributeArguments(nameof(EntityLayoutAttribute), attributeArguments, 2, entity, idsLocation))
                        {
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 0, (value) => { entity.LayoutMethod = (LayoutMethod)value; });
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 1, (value) => { entity.BlockLength = value; });
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
                        member.Parent = entity;
                        if (pdsSymbol.Type is INamedTypeSymbol pdsSymbolType)
                        {
                            member.MemberTypeName = pdsSymbolType.Name;
                            member.MemberIsValueType = pdsSymbolType.IsValueType;
                            member.MemberIsReferenceType = pdsSymbolType.IsReferenceType;
                            if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "ReadOnlyMemory" && pdsSymbolType.TypeArguments.Length == 1)
                            {
                                member.MemberIsArray = true;
                                ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                                member.MemberTypeName = typeArg0.Name;
                                member.MemberIsValueType = typeArg0.IsValueType;
                                member.MemberIsReferenceType = typeArg0.IsReferenceType;
                            }
                            else if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "Nullable" && pdsSymbolType.TypeArguments.Length == 1)
                            {
                                member.MemberIsNullable = true;
                                ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                                member.MemberTypeName = typeArg0.Name;
                                member.MemberIsValueType = typeArg0.IsValueType;
                                member.MemberIsReferenceType = typeArg0.IsReferenceType;
                            }
                            else if (pdsSymbolType.IsReferenceType && pdsSymbolType.NullableAnnotation == NullableAnnotation.Annotated)
                            {
                                // nullable ref type
                                member.MemberIsNullable = true;
                            }
                        }
                        ImmutableArray<AttributeData> allAttributes = pdsSymbol.GetAttributes();
                        if (allAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(ObsoleteAttribute)) is AttributeData obsoleteAttr)
                        {
                            member.IsObsolete = true;
                            var attributeArguments = obsoleteAttr.ConstructorArguments;
                            if (attributeArguments.Length == 1)
                            {
                                TryGetAttributeArgumentValue<string>(member, pdsLocation, attributeArguments, 0, (value) => { member.ObsoleteMessage = value; });
                            }
                            if (attributeArguments.Length == 2)
                            {
                                TryGetAttributeArgumentValue<string>(member, pdsLocation, attributeArguments, 0, (value) => { member.ObsoleteMessage = value; });
                                TryGetAttributeArgumentValue<bool>(member, pdsLocation, attributeArguments, 1, (value) => { member.ObsoleteIsError = value; });
                            }
                        }
                        if (allAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberAttribute)) is AttributeData memberAttr)
                        {
                            member.HasMemberAttribute = true;
                            var attributeArguments = memberAttr.ConstructorArguments;
                            if (CheckAttributeArguments(nameof(MemberAttribute), attributeArguments, 1, member, pdsLocation))
                            {
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 0, (value) => { member.Sequence = value; });
                            }
                        }
                        if (allAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberLayoutAttribute)) is AttributeData memberOffsetAttr)
                        {
                            member.HasMemberLayoutAttribute = true;
                            var attributeArguments = memberOffsetAttr.ConstructorArguments;
                            if (CheckAttributeArguments(nameof(MemberLayoutAttribute), attributeArguments, 3, member, pdsLocation))
                            {
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 0, (value) => { member.FieldOffset = value; });
                                TryGetAttributeArgumentValue<bool>(member, pdsLocation, attributeArguments, 1, (value) => { member.IsBigEndian = value; });
                                TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 2, (value) => { member.ArrayLength = value; });
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
