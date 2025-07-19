using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public interface ITargetFactory
    {
        TargetDomain CreateDomain(string name, Location location);
        TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location);
        TargetMember CreateMember(TargetEntity entity, string name, Location location);
        TargetMember CloneMember(TargetEntity entity, TargetMember source);
    }

    public abstract class SyntaxReceiverBase : ISyntaxContextReceiver
    {
        //private const string DomainAttribute = nameof(DomainAttribute);
        private const string EntityAttribute = nameof(EntityAttribute);
        private const string MemberAttribute = nameof(MemberAttribute);
        private const string IdAttribute = nameof(IdAttribute);

        private readonly ITargetFactory _factory;
        public ITargetFactory Factory => _factory;

        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;

        protected SyntaxReceiverBase(ITargetFactory factory)
        {
            _factory = factory;
            _domain = factory.CreateDomain(TypeFullName.DefaultBase.NameSpace, Location.None);
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

        private TypeFullName TryCreateBaseEntity(INamedTypeSymbol parentSymbol, Location parentLocation, ConcurrentBag<SyntaxDiagnostic> syntaxErrors)
        {
            if (parentSymbol.Interfaces.Length == 1)
            {
                INamedTypeSymbol intf = parentSymbol.Interfaces[0];
                TypeFullName bTFN = TypeFullName.Create(intf);
                if (bTFN.IsGeneric)
                {
                    // recursively create open entities
                    TargetEntity bEntity;
                    if (bTFN.IsClosed)
                    {
                        // todo TryAdd
                        bEntity = Domain.ClosedEntities.GetOrAdd(bTFN.FullName, (n) => _factory.CreateEntity(Domain, bTFN, parentLocation));
                        bEntity.BaseName = TryCreateBaseEntity(intf, parentLocation, syntaxErrors);
                    }
                    else
                    {
                        bEntity = Domain.OpenEntities.GetOrAdd(bTFN.FullName, (n) => _factory.CreateEntity(Domain, bTFN, parentLocation));
                    }
                    //bEntity.HasEntityAttribute = true;
                }
                return bTFN;
            }
            return TypeFullName.DefaultBase;
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
                var eTFN = TypeFullName.Create(ids1Symbol);
                TargetEntity entity;
                if (eTFN.IsClosed)
                    entity = Domain.ClosedEntities.GetOrAdd(eTFN.FullName, (n) => _factory.CreateEntity(Domain, eTFN, idsLocation));
                else
                    entity = Domain.OpenEntities.GetOrAdd(eTFN.FullName, (n) => _factory.CreateEntity(Domain, eTFN, idsLocation));
                if (ids1Symbol.Interfaces.Length > 1)
                {
                    // too many interfaces!
                    entity.SyntaxErrors.Add(
                        new SyntaxDiagnostic(
                            DiagnosticId.DTOM0008, "Invalid base name(s)", DiagnosticCategory.Design, idsLocation, DiagnosticSeverity.Error,
                            $"This interface can only implement one optional other interface."));
                }
                ImmutableArray<AttributeData> entityAttributes = ids1Symbol.GetAttributes();
                if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == EntityAttribute) is AttributeData)
                {
                    // found entity attribute
                    //entity.HasEntityAttribute = true;
                    // entity base
                    entity.BaseName = TryCreateBaseEntity(ids1Symbol, idsLocation, entity.SyntaxErrors);
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
                else
                {
                    //
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
                Location pdsLocation = Location.Create(pds.SyntaxTree, pds.Span);
                var eTFN = TypeFullName.Create(ids2Symbol);
                TargetEntity entity;
                if ((eTFN.IsClosed && Domain.ClosedEntities.TryGetValue(eTFN.FullName, out entity)) ||
                    !eTFN.IsClosed && Domain.OpenEntities.TryGetValue(eTFN.FullName, out entity))
                {
                    var member = entity.Members.GetOrAdd(pds.Identifier.Text, (n) => _factory.CreateMember(entity, n, pdsLocation));
                    if (pdsSymbol.Type is INamedTypeSymbol pdsNamedType)
                    {
                        var mTFN = TypeFullName.Create(pdsNamedType);
                        if (mTFN.IsGeneric)
                        {
                            TargetEntity mEntity;
                            if (mTFN.IsClosed)
                            {
                                // todo TryAdd
                                mEntity = Domain.ClosedEntities.GetOrAdd(mTFN.FullName, (n) => _factory.CreateEntity(Domain, mTFN, pdsLocation));
                                mEntity.BaseName = TryCreateBaseEntity(pdsNamedType, pdsLocation, member.SyntaxErrors);
                            }
                            else
                            {
                                mEntity = Domain.OpenEntities.GetOrAdd(mTFN.FullName, (n) => _factory.CreateEntity(Domain, mTFN, pdsLocation));
                            }
                        }
                        member.MemberType = mTFN;
                        member.Kind = mTFN.MemberKind;

                        if (member.MemberType.FullName == FullTypeName.MemoryOctetsqqq)
                        {
                            // binary
                            member.Kind = MemberKind.Binary;
                        }
                        else if (member.MemberType.FullName == FullTypeName.SystemString)
                        {
                            // string
                            member.Kind = MemberKind.String;
                        }
                        else if (pdsNamedType.IsGenericType && pdsNamedType.Name == "ReadOnlyMemory" && pdsNamedType.TypeArguments.Length == 1)
                        {
                            member.Kind = MemberKind.Vector;
                            ITypeSymbol typeArg0 = pdsNamedType.TypeArguments[0];
                            member.MemberType = TypeFullName.Create(typeArg0);
                        }
                        else if (pdsNamedType.IsGenericType && pdsNamedType.Name == "Nullable" && pdsNamedType.TypeArguments.Length == 1)
                        {
                            // nullable value type
                            member.MemberIsNullable = true;
                            ITypeSymbol typeArg0 = pdsNamedType.TypeArguments[0];
                            member.MemberType = TypeFullName.Create(typeArg0);
                            member.Kind = member.MemberType.MemberKind;
                        }

                        if (pdsNamedType.IsReferenceType && pdsNamedType.NullableAnnotation == NullableAnnotation.Annotated)
                        {
                            // nullable ref type
                            member.MemberIsNullable = true;
                        }
                    }
                    else if (pdsSymbol.Type is ITypeSymbol pdsType)
                    {
                        // generic type parameter?
                        var mTFN = TypeFullName.Create(pdsType);
                        member.MemberType = mTFN;
                        if (pdsType.NullableAnnotation == NullableAnnotation.Annotated)
                        {
                            // nullable ref type
                            member.MemberIsNullable = true;
                        }
                    }
                    else
                    {
                        // unknown 
                        member.Kind = MemberKind.Unknown;
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
