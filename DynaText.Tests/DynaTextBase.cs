using System;
using System.IO;

namespace DTOMaker.Gentime.Tests
{
    public abstract class DynaTextBase : IDynaText, IEquatable<DynaTextBase>
    {
        protected DynaMap _map = new DynaMap();
        public DynaMap GetMap() => _map;
        public void LoadFrom(DynaMap map) => _map = map;
        public bool Emit(TextWriter writer, int indent) => _map.Emit(writer, indent);
        public void LoadFrom(string text) => _map = DynaMap.LoadFrom(text);
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