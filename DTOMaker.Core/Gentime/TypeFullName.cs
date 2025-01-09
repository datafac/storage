using System;

namespace DTOMaker.Gentime
{
    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        // todo choose a suitable common namespace
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
    }
}
