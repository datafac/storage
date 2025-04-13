using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DTOMaker.Gentime
{
    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        private static readonly TypeFullName _defaultBase = new TypeFullName("DTOMaker.Runtime", "EntityBase");
        public static TypeFullName DefaultBase => _defaultBase;

        private readonly string _nameSpace;
        private readonly string? _openName;
        private readonly string? _closedName;
        private readonly string _fullName;

        public string NameSpace => _nameSpace;
        public string ShortName => _closedName ?? _openName ?? "";
        public string ShortImplName => ShortName;
        public string ShortIntfName => ShortName;
        public string FullName => _fullName;
        public bool IsGeneric => _openName is not null;
        public bool IsClosed => _closedName is not null;

        public TypeFullName(string nameSpace, string name)
        {
            _nameSpace = nameSpace;
            _openName = null;
            _closedName = name;
            _fullName = _nameSpace + "." + _closedName;
        }

        public TypeFullName(string nameSpace, string openName, string? closedName)
        {
            _nameSpace = nameSpace;
            _openName = openName;
            _closedName = closedName;
            _fullName = _nameSpace + "." + (_closedName ?? _openName);
        }

        public TypeFullName AsOpenGeneric()
        {
            if (_openName is null) throw new InvalidOperationException("Cannot return open type of non-generic type");
            return new TypeFullName(_nameSpace, _openName, null);
        }

        public bool Equals(TypeFullName other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName left, TypeFullName right) => left.Equals(right);
        public static bool operator !=(TypeFullName left, TypeFullName right) => !left.Equals(right);
        public override string ToString() => _fullName;

        public static TypeFullName Create(string nameSpace, INamedTypeSymbol ids)
        {
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
