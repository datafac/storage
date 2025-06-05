using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DTOMaker.Gentime
{
    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        private readonly string _nameSpace;
        private readonly string _name;
        private readonly ImmutableArray<ITypeParameterSymbol> _typeParameters; // generics only
        private readonly ImmutableArray<ITypeSymbol> _typeArguments; // closed generics only
        private readonly string _fullName;
        private readonly int _syntheticId;
        private readonly MemberKind _memberKind;

        private static readonly TypeFullName _defaultBase = new TypeFullName("DTOMaker.Runtime", "EntityBase",
            ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        public static TypeFullName DefaultBase => _defaultBase;

        public static TypeFullName Create(ITypeSymbol ids)
        {
            string entityName;
            if (ids.TypeKind == TypeKind.Interface)
            {
                entityName = ids.Name.Substring(1);
            }
            else
            {
                entityName = ids.Name;
            }
            return new TypeFullName(ids.ContainingNamespace.ToDisplayString(), entityName, ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        }

        public static TypeFullName Create(INamedTypeSymbol ids)
        {
            string nameSpace = ids.ContainingNamespace.ToDisplayString();
            if (ids.TypeKind != TypeKind.Interface)
            {
                return new TypeFullName(nameSpace, ids.Name, ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
            }
            else
            {
                string entityName = ids.Name.Substring(1);
                return new TypeFullName(nameSpace, entityName, ids.TypeParameters, ids.TypeArguments);
            }
        }

        private static (int syntheticId, MemberKind kind) GetSyntheticId(string fullname)
        {
            return fullname switch
            {
                FullTypeName.SystemBoolean => (9001, MemberKind.Native),
                FullTypeName.SystemSByte => (9002, MemberKind.Native),
                FullTypeName.SystemByte => (9003, MemberKind.Native),
                FullTypeName.SystemInt16 => (9004, MemberKind.Native),
                FullTypeName.SystemUInt16 => (9005, MemberKind.Native),
                FullTypeName.SystemChar => (9006, MemberKind.Native),
                FullTypeName.SystemHalf => (9007, MemberKind.Native),
                FullTypeName.SystemInt32 => (9008, MemberKind.Native),
                FullTypeName.SystemUInt32 => (9009, MemberKind.Native),
                FullTypeName.SystemSingle => (9010, MemberKind.Native),
                FullTypeName.SystemInt64 => (9011, MemberKind.Native),
                FullTypeName.SystemUInt64 => (9012, MemberKind.Native),
                FullTypeName.SystemDouble => (9013, MemberKind.Native),
                FullTypeName.SystemInt128 => (9014, MemberKind.Native),
                FullTypeName.SystemUInt128 => (9015, MemberKind.Native),
                FullTypeName.SystemGuid => (9016, MemberKind.Native),
                FullTypeName.SystemDecimal => (9017, MemberKind.Native),
                FullTypeName.SystemString => (9014, MemberKind.String),
                FullTypeName.MemoryOctets => (9099, MemberKind.Binary),
                _ => (0, MemberKind.Unknown),
            };
        }

        private TypeFullName(string nameSpace, string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            _nameSpace = nameSpace;
            _name = name;
            _typeParameters = typeParameters;
            _typeArguments = typeArguments;
            _fullName = nameSpace + "." + MakeCSImplName(name, typeParameters, typeArguments);
            (_syntheticId, _memberKind) = GetSyntheticId(_fullName);
        }

        public string NameSpace => _nameSpace;
        public string ShortImplName => MakeCSImplName(_name, _typeParameters, _typeArguments);
        public string ShortIntfName => MakeCSIntfName(_name, _typeParameters, _typeArguments);
        public string FullName => _fullName;
        public int SyntheticId => _syntheticId;
        public MemberKind MemberKind => _memberKind;
        public bool IsGeneric => _typeParameters.Length > 0;
        public bool IsClosed => (_typeArguments.Length == _typeParameters.Length)
                                && _typeArguments.All(ta => ta.Kind != SymbolKind.TypeParameter);

        public ImmutableArray<ITypeParameterSymbol> TypeParameters => _typeParameters;
        public ImmutableArray<ITypeSymbol> TypeArguments => _typeArguments;
        public TypeFullName AsOpenGeneric()
        {
            return new TypeFullName(_nameSpace, _name, _typeParameters, ImmutableArray<ITypeSymbol>.Empty);
        }

        public bool Equals(TypeFullName other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName left, TypeFullName right) => left.Equals(right);
        public static bool operator !=(TypeFullName left, TypeFullName right) => !left.Equals(right);
        public override string ToString() => _fullName;

        /// <summary>
        /// Creates a unique entity name with closed generic arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeParameters"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        private static string MakeCSImplName(string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            if (typeParameters.Length == 0) return name;

            StringBuilder result = new StringBuilder();
            result.Append(name);
            result.Append('_');
            result.Append(typeParameters.Length);
            for (int i = 0; i < typeParameters.Length; i++)
            {
                result.Append('_');
                if (i < typeArguments.Length)
                {
                    var aTFN = TypeFullName.Create(typeArguments[i]);
                    //var ta = typeArguments[i];
                    result.Append(aTFN.ShortImplName);
                }
                else
                {
                    var tp = typeParameters[i];
                    result.Append(tp.Name);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Reconstructs the open or closed CSharp interface name. This should be the same as that given in the source model.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeParameters"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        private static string MakeCSIntfName(string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            if (typeParameters.Length == 0) return name;

            StringBuilder result = new StringBuilder();
            result.Append(name);
            result.Append('<');
            for (int i = 0; i < typeParameters.Length; i++)
            {
                if (i > 0) result.Append(", ");
                if (i < typeArguments.Length)
                {
                    var ta = typeArguments[i];
                    result.Append(ta.Name);
                }
                else
                {
                    var tp = typeParameters[i];
                    result.Append(tp.Name);
                }
            }
            result.Append('>');
            return result.ToString();
        }
    }
}
