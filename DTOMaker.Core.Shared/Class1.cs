using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DTOMaker.Gentime
{
    // These types mimic attributes in DTOMaker.Models and must be kept in sync.
    public readonly struct DomainAttribute { }
    public readonly struct EntityAttribute { }
    public readonly struct MemberAttribute { }
    public readonly struct IdAttribute { }
    internal static class DiagnosticId
    {
        public const string DTOM0001 = nameof(DTOM0001); // Invalid interface name
        public const string DTOM0002 = nameof(DTOM0002); // Invalid argument count
        public const string DTOM0003 = nameof(DTOM0003); // Invalid member sequence
        public const string DTOM0004 = nameof(DTOM0004); // Invalid member datatype
        public const string DTOM0005 = nameof(DTOM0005); // Invalid argument value
        public const string DTOM0006 = nameof(DTOM0006); // Missing [Entity] attribute
        public const string DTOM0007 = nameof(DTOM0007); // Missing [Member] attribute
        public const string DTOM0008 = nameof(DTOM0008); // Invalid base name(s)
        public const string DTOM0009 = nameof(DTOM0009); // Duplicate entity id
    }
    public static class DiagnosticCategory
    {
        public const string Design = "DTOMaker.Design";
        public const string Naming = "DTOMaker.Naming";
        public const string Syntax = "DTOMaker.Syntax";
        public const string Other = "DTOMaker.Other";
    }
    public sealed class SyntaxDiagnostic
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Category;
        public readonly Location Location;
        public readonly DiagnosticSeverity Severity;
        public readonly string Message;
        public SyntaxDiagnostic(string id, string title, string category, Location location, DiagnosticSeverity severity, string message)
        {
            Id = id;
            Title = title;
            Category = category;
            Location = location;
            Message = message;
            Severity = severity;
        }
    }
    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        private static readonly TypeFullName _defaultBase = new TypeFullName("DTOMaker.Runtime", "EntityBase");
        public static TypeFullName DefaultBase => _defaultBase;

        private readonly string _nameSpace;
        private readonly string _shortName;
        private readonly string _fullName;

        public string NameSpace => _nameSpace;
        public string ShortName => _shortName;
        public string FullName => _fullName;

        public TypeFullName(string nameSpace, string name)
        {
            _nameSpace = nameSpace;
            _shortName = name;
            _fullName = _nameSpace + "." + _shortName;
        }

        public bool Equals(TypeFullName other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName left, TypeFullName right) => left.Equals(right);
        public static bool operator !=(TypeFullName left, TypeFullName right) => !left.Equals(right);

        public override string ToString() => _fullName;

        public TypeFullName WithShortName(Func<string, string> modifier) => new TypeFullName(_nameSpace, modifier(_shortName));
    }
    public abstract class TargetBase
    {
        public Location Location { get; }
        public ConcurrentBag<SyntaxDiagnostic> SyntaxErrors { get; } = new ConcurrentBag<SyntaxDiagnostic>();
        protected TargetBase(Location location)
        {
            Location = location;
        }

        protected abstract IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics();
        public IEnumerable<SyntaxDiagnostic> ValidationErrors() => OnGetValidationDiagnostics();
    }
    public abstract class TargetDomain : TargetBase
    {
        public string Name { get; }
        public ConcurrentDictionary<string, TargetEntity> Entities { get; } = new ConcurrentDictionary<string, TargetEntity>();
        public TargetDomain(string name, Location location) : base(location)
        {
            Name = name;
        }

        private SyntaxDiagnostic? CheckEntityIdsAreUnique()
        {
            Dictionary<string, TargetEntity> idMap = new Dictionary<string, TargetEntity>();

            foreach (var entity in this.Entities.Values.OrderBy(e => e.EntityName.FullName))
            {
                string id = entity.EntityId;
                if (idMap.TryGetValue(id, out var otherEntity))
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0009, "Duplicate entity id", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Entity id ({id}) is already used by entity: {otherEntity.EntityName}");
                }
                idMap[id] = entity;
            }

            return null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckEntityIdsAreUnique()) is not null) yield return diagnostic;
        }
    }
    public abstract class TargetEntity : TargetBase
    {
        public TypeFullName EntityName { get; }

        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;
        public ConcurrentDictionary<string, TargetMember> Members { get; } = new ConcurrentDictionary<string, TargetMember>();
        public TargetEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(location)
        {
            EntityName = new TypeFullName(nameSpace, name);
            _domain = domain;
        }
        public string EntityId { get; set; } = "_undefined_entity_id_";
        public bool HasEntityAttribute { get; set; }
        public TypeFullName BaseName { get; set; } = TypeFullName.DefaultBase;
        public TargetEntity? Base { get; set; }
        public TargetEntity[] DerivedEntities { get; set; } = [];

        public int GetClassHeight() => Base is not null ? Base.GetClassHeight() + 1 : 1;

        private SyntaxDiagnostic? CheckHasEntityAttribute()
        {
            if (HasEntityAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0006, "Missing [Entity] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Entity] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequenceIsValid()
        {
            int expectedSequence = 1;
            foreach (var member in Members.Values.OrderBy(m => m.Sequence))
            {
                if (member.Sequence != expectedSequence)
                    return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"Expected member '{member.Name}' sequence to be {expectedSequence}, but found {member.Sequence}.");
                expectedSequence++;
            }
            return null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasEntityAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequenceIsValid()) is not null) yield return diagnostic;
        }
    }
    public abstract class TargetMember : TargetBase
    {
        private readonly string _name;
        private readonly TargetEntity _entity;

        public string Name => _name;
        public TargetEntity Entity => _entity;
        public TargetMember(TargetEntity entity, string name, Location location) : base(location)
        {
            _entity = entity;
            _name = name;
        }
        public bool HasMemberAttribute { get; set; }
        public TypeFullName MemberType { get; set; }
        public bool MemberIsValueType { get; set; }
        public bool MemberIsReferenceType { get; set; }
        public bool MemberIsNullable { get; set; }
        public bool IsObsolete { get; set; }
        public string ObsoleteMessage { get; set; } = "";
        public bool ObsoleteIsError { get; set; }
        public int Sequence { get; set; }
        public bool MemberIsVector { get; set; }
        public bool MemberIsEntity { get; set; }
        public int FieldLength { get; set; }

        private SyntaxDiagnostic? CheckHasMemberAttribute()
        {
            if (HasMemberAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0007, "Missing [Member] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Member] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequence()
        {
            if (!HasMemberAttribute) return null;
            return Sequence <= 0
                ? new SyntaxDiagnostic(
                    DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"Sequence ({Sequence}) must be > 0")
                : null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasMemberAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
        }
    }
    public interface ILanguage
    {
        string CommentPrefix { get; }
        string CommandPrefix { get; }
        string TokenPrefix { get; }
        string TokenSuffix { get; }
        string GetDataTypeToken(TypeFullName typeFullName);
        string GetDefaultValue(TypeFullName typeFullName);
        string GetValueAsCode(object? value);
    }
    public interface IModelScope
    {
        IReadOnlyDictionary<string, object?> Tokens { get; }
        (bool?, IModelScope[]) GetInnerScopes(string iteratorName);
    }
    public interface IScopeFactory
    {
        ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity);
        ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member);
    }
    public abstract class ModelScopeBase : IModelScope
    {
        protected readonly IModelScope _parent;
        protected readonly IScopeFactory _factory;
        protected readonly ILanguage _language;
        protected readonly Dictionary<string, object?> _tokens = new Dictionary<string, object?>();
        public IReadOnlyDictionary<string, object?> Tokens => _tokens;

        protected ModelScopeBase(IModelScope parent, IScopeFactory factory, ILanguage language)
        {
            _parent = parent;
            _factory = factory;
            _language = language;
            foreach (var token in parent.Tokens)
            {
                _tokens[token.Key] = token.Value;
            }
        }

        protected abstract (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName);
        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => OnGetInnerScopes(iteratorName);
    }
    public abstract class ModelScopeDomain : ModelScopeBase
    {
        private readonly TargetDomain _domain;

        public ModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language)
        {
            _domain = domain;
            _tokens["DomainName"] = domain.Name;
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "entities":
                    var entities = _domain.Entities.Values
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
                    if (entities.Length > 0)
                        return (true, entities);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
    public abstract class ModelScopeEntity : ModelScopeBase
    {
        protected readonly TargetEntity _entity;
        public readonly int DerivedEntityCount;
        public readonly int ClassHeight;

        public ModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language)
        {
            DerivedEntityCount = entity.DerivedEntities.Length;
            ClassHeight = entity.GetClassHeight();

            _entity = entity;
            _tokens["NameSpace"] = entity.EntityName.NameSpace;
            _tokens["EntityName"] = entity.EntityName.ShortName;
            _tokens["EntityName2"] = entity.EntityName.ShortName;
            _tokens["EntityId"] = entity.EntityId;
            _tokens["BaseName"] = entity.Base?.EntityName.ShortName ?? TypeFullName.DefaultBase.ShortName;
            _tokens["BaseNameSpace"] = entity.Base?.EntityName.NameSpace ?? TypeFullName.DefaultBase.NameSpace;
            _tokens["BaseFullName"] = entity.Base?.EntityName.FullName ?? TypeFullName.DefaultBase.FullName;
            _tokens["ClassHeight"] = ClassHeight;
            _tokens["DerivedEntityCount"] = DerivedEntityCount;
        }

        private static bool IsDerivedFrom(TargetEntity candidate, TargetEntity parent)
        {
            if (ReferenceEquals(candidate, parent)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.EntityName.Equals(parent.EntityName)) return true;
            return IsDerivedFrom(candidate.Base, parent);
        }

        public ModelScopeMember[] Members
        {
            get
            {
                return _entity.Members.Values
                            .OrderBy(m => m.Sequence)
                            .Select(m => _factory.CreateMember(this, _factory, _language, m))
                            .ToArray();
            }
        }

        public ModelScopeEntity[] DerivedEntities
        {
            get
            {
                return _entity.DerivedEntities
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
            }
        }

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            switch (iteratorName.ToLowerInvariant())
            {
                case "members":
                    var members = _entity.Members.Values
                            .OrderBy(m => m.Sequence)
                            .Select(m => _factory.CreateMember(this, _factory, _language, m))
                            .ToArray();
                    if (members.Length > 0)
                        return (true, members);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                case "derivedentities":
                    var derivedEntities = _entity.DerivedEntities
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
                    if (derivedEntities.Length > 0)
                        return (true, derivedEntities);
                    else
                        return (false, new IModelScope[] { ModelScopeEmpty.Instance });
                default:
                    return (null, Array.Empty<IModelScope>());
            }
        }
    }
    public abstract class ModelScopeMember : ModelScopeBase
    {
        private readonly TargetMember _member;
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language)
        {
            _member = member;
            _tokens["MemberIsObsolete"] = member.IsObsolete;
            _tokens["MemberObsoleteMessage"] = member.ObsoleteMessage;
            _tokens["MemberObsoleteIsError"] = member.ObsoleteIsError;
            _tokens["MemberType"] = _language.GetDataTypeToken(member.MemberType);
            _tokens["MemberTypeName"] = member.MemberType.ShortName;
            _tokens["MemberTypeNameSpace"] = member.MemberType.NameSpace;
            _tokens["MemberIsNullable"] = member.MemberIsNullable;
            _tokens["MemberIsValueType"] = member.MemberIsValueType;
            _tokens["MemberIsReferenceType"] = member.MemberIsReferenceType;
            _tokens["MemberIsVector"] = member.MemberIsVector;
            _tokens["MemberSequence"] = member.Sequence;
            _tokens["ScalarMemberSequence"] = member.Sequence;
            if (member.MemberIsNullable)
                _tokens["NullableScalarMemberSequence"] = member.Sequence;
            else
                _tokens["RequiredScalarMemberSequence"] = member.Sequence;
            _tokens["VectorMemberSequence"] = member.Sequence;
            _tokens["MemberName"] = member.Name;
            _tokens["ScalarMemberName"] = member.Name;
            if (member.MemberIsNullable)
                _tokens["NullableScalarMemberName"] = member.Name;
            else
                _tokens["RequiredScalarMemberName"] = member.Name;
            _tokens["VectorMemberName"] = member.Name;
            _tokens["MemberJsonName"] = member.Name.ToCamelCase();
            _tokens["MemberDefaultValue"] = _language.GetDefaultValue(member.MemberType);
            _tokens["MemberIsEntity"] = member.MemberIsEntity;
            if (member.MemberIsEntity)
            {
                if (member.MemberIsNullable)
                    _tokens["NullableEntityMemberName"] = member.Name;
                else
                    _tokens["RequiredEntityMemberName"] = member.Name;
            }
        }

        public bool IsEntity => _member.MemberIsEntity;
        public bool IsNullable => _member.MemberIsNullable;
        public bool IsVector => _member.MemberIsVector;
        public bool IsObsolete => _member.IsObsolete;
        public int FieldLength => _member.FieldLength;

        protected override (bool?, IModelScope[]) OnGetInnerScopes(string iteratorName)
        {
            return (null, Array.Empty<IModelScope>());
        }
    }
    public sealed class ModelScopeEmpty : IModelScope
    {
        private static readonly ModelScopeEmpty _instance = new ModelScopeEmpty();
        public static ModelScopeEmpty Instance => _instance;

        public IReadOnlyDictionary<string, object?> Tokens { get; } = new Dictionary<string, object?>();

        private ModelScopeEmpty() { }

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
    public abstract class SourceGeneratorBase : ISourceGenerator
    {
        protected abstract void OnInitialize(GeneratorInitializationContext context);
        public void Initialize(GeneratorInitializationContext context) => OnInitialize(context);

        private static bool IsDerivedFrom(TargetEntity candidate, TargetEntity parent)
        {
            if (ReferenceEquals(candidate, parent)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.EntityName.Equals(parent.EntityName)) return true;
            return IsDerivedFrom(candidate.Base, parent);
        }

        protected abstract void OnExecute(GeneratorExecutionContext context);
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiverBase syntaxReceiver) return;

            // fix entity hierarchy
            var domain = syntaxReceiver.Domain;
            // fix/set entity base
            var entities = domain.Entities.Values.ToArray();
            foreach (var entity in entities)
            {
                if (!entity.BaseName.Equals(TypeFullName.DefaultBase))
                {
                    if (domain.Entities.TryGetValue(entity.BaseName.FullName, out var baseEntity))
                    {
                        entity.Base = baseEntity;
                    }
                    else
                    {
                        // invalid base name!
                        entity.SyntaxErrors.Add(
                            new SyntaxDiagnostic(
                                DiagnosticId.DTOM0008, "Invalid base name", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                                $"Base name '{entity.BaseName}' does not refer to a known entity."));
                    }
                }
            }

            // determine derived entities
            foreach (var entity in entities)
            {
                entity.DerivedEntities = domain.Entities.Values
                    .Where(e => IsDerivedFrom(e, entity))
                    .OrderBy(e => e.EntityName.FullName)
                    .ToArray();
            }

            // determine entity members
            foreach (var entity in entities)
            {
                foreach (var member in entity.Members.Values)
                {
                    var entity2 = entities.FirstOrDefault(e => e.EntityName.WithShortName(sn => "I" + sn) == member.MemberType);
                    if (entity2 is not null)
                    {
                        member.MemberIsEntity = true;
                        member.MemberType = entity2.EntityName;
                    }
                }
            }

            OnExecute(context);
        }
    }
    public abstract class SyntaxReceiverBase : ISyntaxContextReceiver
    {
        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;

        private readonly Func<TargetDomain, string, string, Location, TargetEntity> _entityFactory;
        private readonly Func<TargetEntity, string, Location, TargetMember> _memberFactory;

        protected SyntaxReceiverBase(
            Func<string, Location, TargetDomain> domainFactory,
            Func<TargetDomain, string, string, Location, TargetEntity> entityFactory,
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
                ? $"Could not parse arg[{index}] (null) as <{typeof(T).Name}>"
                : $"Could not parse arg[{index}] '{input}' <{input.GetType().Name}> as <{typeof(T).Name}>";

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

        protected virtual void OnProcessNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InterfaceDeclarationSyntax ids
                && ids.Modifiers.Any(SyntaxKind.PublicKeyword)
                && context.SemanticModel.GetDeclaredSymbol(ids) is INamedTypeSymbol idsSymbol)
            {
                if (ids.Parent is NamespaceDeclarationSyntax nds && ids.AttributeLists.Count > 0)
                {
                    Location ndsLocation = Location.Create(nds.SyntaxTree, nds.Span);
                    Location idsLocation = Location.Create(ids.SyntaxTree, ids.Span);
                    string entityNamespace = nds.Name.ToString();
                    string interfaceName = ids.Identifier.Text;
                    if (interfaceName.Length <= 1 || !interfaceName.StartsWith("I"))
                    {
                        Domain.SyntaxErrors.Add(
                            new SyntaxDiagnostic(
                                DiagnosticId.DTOM0001, "Invalid interface name", DiagnosticCategory.Naming, idsLocation, DiagnosticSeverity.Error,
                                $"Expected interface named '{interfaceName}' to start with 'I'."));
                    }
                    string entityName = interfaceName.Substring(1);
                    string entityFullName = entityNamespace + "." + entityName;
                    var entity = Domain.Entities.GetOrAdd(entityFullName, (n) => _entityFactory(Domain, entityNamespace, entityName, idsLocation));
                    ImmutableArray<AttributeData> entityAttributes = idsSymbol.GetAttributes();
                    if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(EntityAttribute)) is AttributeData entityAttr)
                    {
                        // found entity attribute
                        entity.HasEntityAttribute = true;
                        // entity base
                        entity.BaseName = TypeFullName.DefaultBase;
                        if (idsSymbol.Interfaces.Length > 1)
                        {
                            // too many interfaces!
                            entity.SyntaxErrors.Add(
                                new SyntaxDiagnostic(
                                    DiagnosticId.DTOM0008, "Invalid base name(s)", DiagnosticCategory.Design, idsLocation, DiagnosticSeverity.Error,
                                    $"This interface can only implement one optional other interface."));
                        }
                        else if (idsSymbol.Interfaces.Length == 1)
                        {
                            var intf = idsSymbol.Interfaces[0];
                            string baseNameSpace = intf.ContainingNamespace.ToDisplayString();
                            string baseName = intf.Name;
                            if (baseName.Length <= 1 || !baseName.StartsWith("I"))
                            {
                                entity.SyntaxErrors.Add(
                                    new SyntaxDiagnostic(
                                        DiagnosticId.DTOM0001, "Invalid base name", DiagnosticCategory.Naming, idsLocation, DiagnosticSeverity.Error,
                                        $"Expected interface named '{baseName}' to start with 'I'."));
                            }
                            else
                            {
                                entity.BaseName = new TypeFullName(baseNameSpace, baseName.Substring(1));
                            }
                        }
                        entity.EntityId = entity.EntityName.FullName;
                        if (entityAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(IdAttribute)) is AttributeData idAttr)
                        {
                            // found entity id attribute
                            var attributeArguments = idAttr.ConstructorArguments;
                            if (CheckAttributeArguments(nameof(IdAttribute), attributeArguments, 1, entity, idsLocation))
                            {
                                TryGetAttributeArgumentValue<string>(entity, idsLocation, attributeArguments, 0, (value) => { entity.EntityId = value; });
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
            }

            if (context.Node is PropertyDeclarationSyntax pds
                && context.SemanticModel.GetDeclaredSymbol(pds) is IPropertySymbol pdsSymbol)
            {
                if (pds.Parent is InterfaceDeclarationSyntax ids2
                    && ids2.Parent is NamespaceDeclarationSyntax nds2
                    && pds.AttributeLists.Count > 0)
                {
                    string entityNamespace = nds2.Name.ToString();
                    string entityName = ids2.Identifier.Text.Substring(1);
                    string entityFullName = entityNamespace + "." + entityName;
                    if (Domain.Entities.TryGetValue(entityFullName, out var entity)
                        && pdsSymbol.Type is INamedTypeSymbol pdsSymbolType)
                    {
                        string memberTypeName = pdsSymbolType.Name;
                        string memberTypeNameSpace = pdsSymbolType.ContainingNamespace.ToDisplayString();
                        Location pdsLocation = Location.Create(pds.SyntaxTree, pds.Span);
                        var member = entity.Members.GetOrAdd(pds.Identifier.Text, (n) => _memberFactory(entity, n, pdsLocation));
                        member.MemberType = new TypeFullName(memberTypeNameSpace, memberTypeName);
                        member.MemberIsValueType = pdsSymbolType.IsValueType;
                        member.MemberIsReferenceType = pdsSymbolType.IsReferenceType;
                        if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "ReadOnlyMemory" && pdsSymbolType.TypeArguments.Length == 1)
                        {
                            member.MemberIsVector = true;
                            ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                            member.MemberType = new TypeFullName(typeArg0.ContainingNamespace.ToDisplayString(), typeArg0.Name);
                            member.MemberIsValueType = typeArg0.IsValueType;
                            member.MemberIsReferenceType = typeArg0.IsReferenceType;
                        }
                        else if (pdsSymbolType.IsGenericType && pdsSymbolType.Name == "Nullable" && pdsSymbolType.TypeArguments.Length == 1)
                        {
                            member.MemberIsNullable = true;
                            ITypeSymbol typeArg0 = pdsSymbolType.TypeArguments[0];
                            member.MemberType = new TypeFullName(typeArg0.ContainingNamespace.ToDisplayString(), typeArg0.Name);
                            member.MemberIsValueType = typeArg0.IsValueType;
                            member.MemberIsReferenceType = typeArg0.IsReferenceType;
                        }
                        else if (pdsSymbolType.IsReferenceType && pdsSymbolType.NullableAnnotation == NullableAnnotation.Annotated)
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
                        if (allAttributes.FirstOrDefault(a => a.AttributeClass?.Name == nameof(MemberAttribute)) is AttributeData memberAttr)
                        {
                            member.HasMemberAttribute = true;
                            var attributeArguments = memberAttr.ConstructorArguments;
                            if (CheckAttributeArguments(nameof(MemberAttribute), attributeArguments, 1, member, pdsLocation))
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
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            OnProcessNode(context);
        }
    }
    public abstract class EntityGeneratorBase
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly TokenStack _tokenStack = new TokenStack();
        private readonly ILanguage _language;

        protected EntityGeneratorBase(ILanguage language)
        {
            _language = language;
        }

        private string ReplaceTokens(string input)
        {
            // note token recursion not supported
            var tokenPrefix = _language.TokenPrefix.AsSpan();
            var tokenSuffix = _language.TokenSuffix.AsSpan();

            ReadOnlySpan<char> inputSpan = input.AsSpan();

            // fast exit for lines with no tokens
            if (inputSpan.IndexOf(tokenPrefix) < 0) return input;

            StringBuilder result = new StringBuilder();
            bool replaced = false;
            int remainderPos = 0;
            do
            {
                ReadOnlySpan<char> remainder = inputSpan.Slice(remainderPos);
                int tokenPos = remainder.IndexOf(tokenPrefix);
                int tokenEnd = tokenPos < 0 ? -1 : remainder.Slice(tokenPos + tokenPrefix.Length).IndexOf(tokenSuffix);
                if (tokenPos >= 0 && tokenEnd >= 0)
                {
                    // token found!
                    var tokenSpan = remainder.Slice(tokenPos + tokenPrefix.Length, tokenEnd);
                    string tokenName = tokenSpan.ToString();
                    if (_tokenStack.Top.TryGetValue(tokenName, out var tokenValue))
                    {
                        // replace valid token
                        // - emit prefix
                        // - emit token
                        // - calc remainder
                        ReadOnlySpan<char> prefix = remainder.Slice(0, tokenPos);
                        result.Append(prefix.ToString());
                        result.Append(_language.GetValueAsCode(tokenValue));
                        remainderPos += (tokenPos + tokenPrefix.Length + tokenEnd + tokenSuffix.Length);
                        replaced = true;
                    }
                    else
                    {
                        // invalid token - emit error then original line
                        result.Clear();
                        result.AppendLine($"#error The token '{_language.TokenPrefix}{tokenName}{_language.TokenSuffix}' on the following line is invalid.");
                        result.AppendLine(input);
                        return result.ToString();
                    }
                }
                else
                {
                    // no token - emit remainder and return
                    result.Append(remainder.ToString());
                    return result.ToString();
                }
            }
            while (replaced);

            return result.ToString();
        }

        protected void Emit(string line)
        {
            _builder.AppendLine(ReplaceTokens(line));
        }

        protected IDisposable NewScope(ModelScopeBase scope)
        {
            return _tokenStack.NewScope(scope.Tokens);
        }

        protected abstract void OnGenerate(ModelScopeEntity scope);
        public string GenerateSourceText(ModelScopeEntity scope)
        {
            using var _ = NewScope(scope);
            _builder.Clear();
            OnGenerate(scope);
            return _builder.ToString();
        }
    }
    internal sealed class TokenStack
    {
        private class Disposer : IDisposable
        {
            private readonly Stack<ImmutableDictionary<string, object?>> _stack;
            public Disposer(Stack<ImmutableDictionary<string, object?>> stack) => _stack = stack;

            private volatile bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _stack.Pop();
            }
        }

        private readonly Stack<ImmutableDictionary<string, object?>> _stack = new Stack<ImmutableDictionary<string, object?>>();
        public TokenStack() => _stack.Push(ImmutableDictionary<string, object?>.Empty);
        public ImmutableDictionary<string, object?> Top => _stack.Peek();
        public IDisposable NewScope(IReadOnlyDictionary<string, object?> tokens)
        {
            var oldScope = _stack.Peek();
            var newScope = oldScope.SetItems(tokens);
            _stack.Push(newScope);
            return new Disposer(_stack);
        }
    }
    public class Language_CSharp : ILanguage
    {
        private static readonly Language_CSharp _instance = new Language_CSharp();
        public static Language_CSharp Instance => _instance;

        private Language_CSharp()
        {
            CommentPrefix = "//";
            CommandPrefix = "##";
            TokenPrefix = "T_";
            TokenSuffix = "_";
        }

        public string TokenPrefix { get; } = "";
        public string TokenSuffix { get; } = "";
        public string CommentPrefix { get; } = "";
        public string CommandPrefix { get; } = "";

        public string GetValueAsCode(object? value)
        {
            return value switch
            {
                null => "null",
                string s => s,
                bool b => b ? "true" : "false",
                float f => $"{f}F",
                double d => $"{d}D",
                short s => $"{s}S",
                ushort us => $"{us}US",
                int i => $"{i}",
                uint u => $"{u}U",
                long l => $"{l}L",
                ulong ul => $"{ul}UL",
                decimal m => $"{m}M",
                _ => $"{value}"
            };
        }

        public string GetDataTypeToken(TypeFullName typeFullName)
        {
            return typeFullName.ShortName;
        }

        public string GetDefaultValue(TypeFullName typeFullName)
        {
            return typeFullName.ShortName switch
            {
                "String" => "string.Empty",
                //todo NativeType.Binary => "Octets.Empty",
                _ => $"default"
            };
        }

    }
    internal static class InternalExtensions
    {
        public static string ToCamelCase(this string value)
        {
            ReadOnlySpan<char> input = value.AsSpan();
            Span<char> output = stackalloc char[input.Length];
            input.CopyTo(output);
            for (int i = 0; i < output.Length; i++)
            {
                if (Char.IsLetter(output[i]))
                {
                    output[i] = Char.ToLower(output[i]);
                    return new string(output.ToArray());
                }
            }
            return new string(output.ToArray());
        }
    }
}
