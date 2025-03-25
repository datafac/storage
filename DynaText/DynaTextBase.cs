using System;
using System.IO;

namespace DTOMaker.Gentime
{
    public abstract class DynaTextBase : IDynaText, IEquatable<DynaTextBase>
    {
        protected DynaTextMap _map = new DynaTextMap();
        public DynaTextMap GetMap() => _map;
        public void LoadFrom(DynaTextMap map) => _map = map;
        public bool Emit(TextWriter writer, int indent) => _map.Emit(writer, indent);
        public void LoadFrom(string text) => _map = DynaTextMap.LoadFrom(text);
        public bool Equals(DynaTextBase? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return _map.Equals(other._map);
        }
        public override bool Equals(object? obj) => obj is DynaTextBase other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(GetType(), _map.GetHashCode());
    }
}