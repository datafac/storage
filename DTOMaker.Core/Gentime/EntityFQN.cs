using System;

namespace DTOMaker.Gentime
{
    public readonly struct EntityFQN : IEquatable<EntityFQN>
    {
        // todo choose a suitable common namespace
        private static readonly EntityFQN _defaultBase = new EntityFQN("DTOMaker.Runtime", "EntityBase");
        public static EntityFQN DefaultBase => _defaultBase;

        private readonly string _nameSpace;
        private readonly string _shortName;
        private readonly string _fullName;

        public string NameSpace => _nameSpace;
        public string ShortName => _shortName;
        public string FullName => _fullName;

        public EntityFQN(string nameSpace, string name)
        {
            _nameSpace = nameSpace;
            _shortName = name;
            _fullName = _nameSpace + "." + _shortName;
        }

        public bool Equals(EntityFQN other)
        {
            return string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        }

        public override string ToString() => _fullName;
    }
}
