using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DTOMaker.Gentime
{
    public readonly struct TypeFullName2 : IEquatable<TypeFullName2>
    {
        private readonly string _nameSpace;
        private readonly string _name;
        private readonly ImmutableArray<ITypeSymbol> _typeArguments;
        private readonly string _fullName;

        private TypeFullName2(string nameSpace, string name, ImmutableArray<ITypeSymbol> typeArguments)
        {
            _nameSpace = nameSpace;
            _name = name;
            _typeArguments = typeArguments;
            _fullName = nameSpace + "." + MakeCSImplName(name, typeArguments);
        }

        public string NameSpace => _nameSpace;
        public string ShortImplName => MakeCSImplName(_name, _typeArguments);
        public string ShortIntfName => MakeCSIntfName(_name, _typeArguments);
        public string FullName => _fullName;
        public bool IsGeneric => _typeArguments.Length > 0;
        public bool IsClosed
        {
            get
            {
                for (int i = 0; i < _typeArguments.Length; i++)
                {
                    var ta = _typeArguments[i];
                    if (ta.Kind != SymbolKind.NamedType) return false;
                }
                return true;
            }
        }

        public bool Equals(TypeFullName2 other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName2 left, TypeFullName2 right) => left.Equals(right);
        public static bool operator !=(TypeFullName2 left, TypeFullName2 right) => !left.Equals(right);
        public override string ToString() => _fullName;

        private static string MakeCSImplName(string name, ImmutableArray<ITypeSymbol> typeArguments)
        {
            StringBuilder result = new StringBuilder();
            result.Append(name);
            if (typeArguments.Length > 0)
            {
                result.Append('_');
                result.Append(typeArguments.Length);
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    var ta = typeArguments[i];
                    result.Append('_');
                    result.Append(ta.Name);
                }
            }
            return result.ToString();
        }

        private static string MakeCSIntfName(string name, ImmutableArray<ITypeSymbol> typeArguments)
        {
            StringBuilder result = new StringBuilder();
            result.Append(name);
            if (typeArguments.Length > 0)
            {
                result.Append('<');
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    if (i > 0) result.Append(", ");
                    var ta = typeArguments[i];
                    result.Append(ta.Name);
                }
                result.Append('>');
            }
            return result.ToString();
        }
    }

    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        private static readonly TypeFullName _defaultBase = new TypeFullName("DTOMaker.Runtime", "EntityBase");
        public static TypeFullName DefaultBase => _defaultBase;

        private readonly string _nameSpace;
        private readonly bool _isGeneric;
        private readonly string? _openImplName;
        private readonly string? _openIntfName;
        private readonly string? _closedImplName;
        private readonly string? _closedIntfName;
        private readonly string _fullName;

        public string NameSpace => _nameSpace;
        public string ShortImplName => _closedImplName ?? _openImplName ?? "";
        public string ShortIntfName => _closedIntfName ?? _openIntfName ?? "";
        public string FullName => _fullName;
        public bool IsGeneric => _isGeneric;
        public bool IsClosed => _closedImplName is not null;

        private TypeFullName(string nameSpace, string name)
        {
            _nameSpace = nameSpace;
            _isGeneric = false;
            _openImplName = null;
            _openIntfName = null;
            _closedImplName = name;
            _closedIntfName = name;
            _fullName = _nameSpace + "." + _closedImplName;
        }

        private TypeFullName(string nameSpace, string openName, string? closedName)
        {
            _nameSpace = nameSpace;
            _isGeneric = true;
            _openImplName = openName;
            _openIntfName = openName;
            _closedImplName = closedName;
            _closedIntfName = closedName;
            _fullName = _nameSpace + "." + (_closedImplName ?? _openImplName);
        }

        public TypeFullName AsOpenGeneric()
        {
            if (_openImplName is null) throw new InvalidOperationException("Cannot return open type of non-generic type");
            return new TypeFullName(_nameSpace, _openImplName, null);
        }

        public bool Equals(TypeFullName other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName left, TypeFullName right) => left.Equals(right);
        public static bool operator !=(TypeFullName left, TypeFullName right) => !left.Equals(right);
        public override string ToString() => _fullName;

        public static TypeFullName Create(ITypeSymbol ids)
        {
            return new TypeFullName(ids.ContainingNamespace.ToDisplayString(), ids.Name);
        }

        public static TypeFullName Create(INamedTypeSymbol ids)
        {
            string nameSpace = ids.ContainingNamespace.ToDisplayString();
            if (ids.TypeKind != TypeKind.Interface) return new TypeFullName(nameSpace, ids.Name);

            // is entity
            string entityName = ids.Name.Substring(1);
            int typeParameterCount = ids.TypeParameters.Length;
            if (typeParameterCount <= 0) return new TypeFullName(nameSpace, entityName);

            // is generic entity
            string openName = $"{entityName}_{typeParameterCount}";
            if (ids.TypeArguments.Length == typeParameterCount && ids.TypeArguments.All(ta => ta.Kind == SymbolKind.NamedType))
            {
                // closed generic entity
                string closedName = openName;
                for (int i = 0; i < typeParameterCount; i++)
                {
                    var ta = ids.TypeArguments[i];
                    closedName += $"_{ta.Name}";
                }
                return new TypeFullName(nameSpace, openName, closedName);
            }
            else
            {
                // open generic entity
                return new TypeFullName(nameSpace, openName, null);
            }
        }

    }
}
