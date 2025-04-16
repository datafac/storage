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

        private static readonly TypeFullName _defaultBase = new TypeFullName("DTOMaker.Runtime", "EntityBase",
            ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        public static TypeFullName DefaultBase => _defaultBase;

        public static TypeFullName Create(ITypeSymbol ids)
        {
            return new TypeFullName(ids.ContainingNamespace.ToDisplayString(), ids.Name, ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        }

        public static TypeFullName Create(INamedTypeSymbol ids)
        {
            string nameSpace = ids.ContainingNamespace.ToDisplayString();
            if (ids.TypeKind != TypeKind.Interface) return new TypeFullName(nameSpace, ids.Name, ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);

            // is entity
            string entityName = ids.Name.Substring(1);
            return new TypeFullName(nameSpace, entityName, ids.TypeParameters, ids.TypeArguments);
        }

        private TypeFullName(string nameSpace, string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            _nameSpace = nameSpace;
            _name = name;
            _typeParameters = typeParameters;
            _typeArguments = typeArguments;
            _fullName = nameSpace + "." + MakeCSImplName(name, typeParameters, typeArguments);
        }

        public string NameSpace => _nameSpace;
        public string ShortImplName => MakeCSImplName(_name, _typeParameters, _typeArguments);
        public string ShortIntfName => MakeCSIntfName(_name, _typeParameters, _typeArguments);
        public string FullName => _fullName;
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
                    var ta = typeArguments[i];
                    result.Append(ta.Name);
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
