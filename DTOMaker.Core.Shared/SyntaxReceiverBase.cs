using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class SyntaxReceiverBase : ISyntaxContextReceiver
    {
        //private const string DomainAttribute = nameof(DomainAttribute);
        private const string EntityAttribute = nameof(EntityAttribute);
        private const string MemberAttribute = nameof(MemberAttribute);
        private const string IdAttribute = nameof(IdAttribute);

        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;

        private readonly Func<TargetDomain, TypeFullName, Location, TargetEntity> _entityFactory;
        private readonly Func<TargetEntity, string, Location, TargetMember> _memberFactory;

        protected SyntaxReceiverBase(
            Func<string, Location, TargetDomain> domainFactory,
            Func<TargetDomain, TypeFullName, Location, TargetEntity> entityFactory,
            Func<TargetEntity, string, Location, TargetMember> memberFactory)
        {
            _domain = domainFactory(TypeFullName.DefaultBase.NameSpace, Location.None);
            _entityFactory = entityFactory;
            _memberFactory = memberFactory;
        }

        protected static void TryGetAttributeArgumentValue<T>(TargetBase target, Location location, ImmutableArray<TypedConstant> attributeArguments, int index, Action<T> action)
        {
            object? input = attributeArguments[index].Value;
            if (input is T value)
            {
                action(value);
                return;
            }

            string? errorMessage = input is null
                ? $"Could not read arg[{index}] (null) as <{typeof(T).Name}>"
                : $"Could not read arg[{index}] '{input}' <{input.GetType().Name}> as <{typeof(T).Name}>";

            target.SyntaxErrors.Add(
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    errorMessage));
        }

        protected static void TryGetAttributeArgumentValue<TInp, TOut>(TargetBase target, Location location, ImmutableArray<TypedConstant> attributeArguments, int index, Func<TInp, (bool, TOut)> parser, Action<TOut> action)
        {
            string? errorMessage = null;
            object? input = attributeArguments[index].Value;
            if (input is TInp value)
            {
                (bool parsed, TOut output) = parser(value);
                if (parsed)
                {
                    action(output);
                    return;
                }
                else
                {
                    errorMessage = $"Could not convert arg[{index}] '{value}' <{typeof(TInp).Name}> to <{typeof(TOut).Name}>";
                }
            }
            else
            {
                errorMessage = input is null
                    ? $"Could not read arg[{index}] (null) as <{typeof(TInp).Name}>"
                    : $"Could not read arg[{index}] '{input}' <{input.GetType().Name}> as <{typeof(TInp).Name}>";

            }

            target.SyntaxErrors.Add(
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    errorMessage));
        }

        protected static bool CheckAttributeArguments(string attributeName, ImmutableArray<TypedConstant> arguments, int expectedCount, TargetBase target, Location location)
        {
            if (arguments.Length == expectedCount)
                return true;

            target.SyntaxErrors.Add(
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    $"Expected {attributeName} attribute to have {expectedCount} arguments, but it has {arguments.Length}."));

            return false;
        }

        protected abstract void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes);

        protected abstract void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes);

        private static (bool parsed, Guid output) TryParseGuid(string input)
        {
            if (Guid.TryParse(input, out Guid result))
                return (true, result);
            else
                return (false, Guid.Empty);
        }

        protected virtual void OnProcessNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InterfaceDeclarationSyntax ids1
                && ids1.Modifiers.Any(SyntaxKind.PublicKeyword)
                && context.SemanticModel.GetDeclaredSymbol(ids1) is INamedTypeSymbol ids1Symbol 
                && ids1.Parent is NamespaceDeclarationSyntax nds1 
                && ids1.AttributeLists.Count > 0)
            {
                Location ndsLocation = Location.Create(nds1.SyntaxTree, nds1.Span);
                Location idsLocation = Location.Create(ids1.SyntaxTree, ids1.Span);
                var eTFN = TypeFullName.Create(nds1.Name.ToString(), ids1Symbol);
                var entity = Domain.Entities.GetOrAdd(eTFN.FullName, (n) => _entityFactory(Domain, eTFN, idsLocation));
                entity.GenericTypeParams = ids1Symbol.TypeParameters.Length;
                ImmutableArray<AttributeData> entityAttributes = ids1Symbol.GetAttributes();
                if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == EntityAttribute) is AttributeData entityAttr)
                {
                    // found entity attribute
                    entity.HasEntityAttribute = true;
                    // entity base
                    entity.BaseName = TypeFullName.DefaultBase;
                    if (ids1Symbol.Interfaces.Length > 1)
                    {
                        // too many interfaces!
                        entity.SyntaxErrors.Add(
                            new SyntaxDiagnostic(
                                DiagnosticId.DTOM0008, "Invalid base name(s)", DiagnosticCategory.Design, idsLocation, DiagnosticSeverity.Error,
                                $"This interface can only implement one optional other interface."));
                    }
                    else if (ids1Symbol.Interfaces.Length == 1)
                    {
                        var intf = ids1Symbol.Interfaces[0];
                        var bTFN = TypeFullName.Create(intf.ContainingNamespace.ToDisplayString(), intf);
                        if (bTFN.IsGeneric && bTFN.IsClosed)
                        {
                            var closedEntity = Domain.Entities.GetOrAdd(bTFN.FullName, (n) => _entityFactory(Domain, bTFN, idsLocation));
                        }
                        entity.BaseName = bTFN;
                    }
                    if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == IdAttribute) is AttributeData idAttr)
                    {
                        // found entity id attribute
                        var attributeArguments = idAttr.ConstructorArguments;
                        if (CheckAttributeArguments(IdAttribute, attributeArguments, 1, entity, idsLocation))
                        {
                            TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 0, (value) => { entity.EntityId = value; });
                        }
                    }
                    //var attributeArguments = entityAttr.ConstructorArguments;
                    //if (CheckAttributeArguments(nameof(EntityAttribute), attributeArguments, 1, entity, idsLocation))
                    //{
                    //    TryGetAttributeArgumentValue<int>(entity, idsLocation, attributeArguments, 0, (value) => { entity.xxx = value; });
                    //}
                }

                // additional entity attribute processing
                OnProcessEntityAttributes(entity, idsLocation, entityAttributes);
            }

            if (context.Node is PropertyDeclarationSyntax pds
                && context.SemanticModel.GetDeclaredSymbol(pds) is IPropertySymbol pdsSymbol 
                && pds.Parent is InterfaceDeclarationSyntax ids2
                && context.SemanticModel.GetDeclaredSymbol(ids2) is INamedTypeSymbol ids2Symbol
                && ids2.Parent is NamespaceDeclarationSyntax nds2
                && pds.AttributeLists.Count > 0)
            {
                var eTFN = TypeFullName.Create(nds2.Name.ToString(), ids2Symbol);
                if (Domain.Entities.TryGetValue(eTFN.FullName, out var entity)
                    && pdsSymbol.Type is INamedTypeSymbol pdsSymbolType)
                {
                    Location pdsLocation = Location.Create(pds.SyntaxTree, pds.Span);
                    var mTFN = TypeFullName.Create(pdsSymbolType.ContainingNamespace.ToDisplayString(), pdsSymbolType);
                    if (mTFN.IsGeneric && mTFN.IsClosed)
                    {
                        var closedEntity = Domain.Entities.GetOrAdd(mTFN.FullName, (n) => _entityFactory(Domain, mTFN, pdsLocation));
                    }
                    var member = entity.Members.GetOrAdd(pds.Identifier.Text, (n) => _memberFactory(entity, n, pdsLocation));
                    member.MemberType = mTFN;
                    if (member.MemberType.FullName == FullTypeName.MemoryOctets)
                    {
                        // binary
                        member.Kind = MemberKind.Binary;
                    }
                    else if (member.MemberType.FullName == FullTypeName.SystemString)
                    {
                        // string
                        member.Kind = MemberKind.String;
                    }
                    else if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "ReadOnlyMemory" && pdsSymbolType.TypeArguments.Length == 1)
                    {
                        member.Kind = MemberKind.Vector;
                        ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                        member.MemberType = new TypeFullName(typeArg0.ContainingNamespace.ToDisplayString(), typeArg0.Name);
                    }
                    else if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "Nullable" && pdsSymbolType.TypeArguments.Length == 1)
                    {
                        // nullable value type
                        member.MemberIsNullable = true;
                        ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                        member.MemberType = new TypeFullName(typeArg0.ContainingNamespace.ToDisplayString(), typeArg0.Name);
                    }

                    if (pdsSymbolType.IsReferenceType && pdsSymbolType.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        // nullable ref type
                        member.MemberIsNullable = true;
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
                    if (allAttributes.FirstOrDefault(a => a.AttributeClass?.Name == MemberAttribute) is AttributeData memberAttr)
                    {
                        member.HasMemberAttribute = true;
                        var attributeArguments = memberAttr.ConstructorArguments;
                        if (CheckAttributeArguments(MemberAttribute, attributeArguments, 1, member, pdsLocation))
                        {
                            TryGetAttributeArgumentValue<int>(member, pdsLocation, attributeArguments, 0, (value) => { member.Sequence = value; });
                        }
                    }

                    // additional member attribute processing
                    OnProcessMemberAttributes(member, pdsLocation, allAttributes);
                }
                else
                {
                    // ignore orphan member
                }
            }
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            OnProcessNode(context);
        }
    }
}
