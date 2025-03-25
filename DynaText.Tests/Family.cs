using System;
using System.IO;

namespace DynaText.Tests
{
    internal class Family : IEmitText, ILoadText, IEquatable<Family>, IMapBacked
    {
        #region boilerplate
        private DynaTextMap _map = new DynaTextMap();
        public DynaTextMap GetMap() => _map;
        public void LoadFrom(DynaTextMap map) => _map = map;
        public bool Emit(TextWriter writer, int indent) => _map.Emit(writer, indent);
        public void LoadFrom(string text) => _map = DynaTextMap.LoadFrom(text);
        public bool Equals(Family? other) => other is not null && (ReferenceEquals(this, other) || _map.Equals(other._map));
        public override bool Equals(object? obj) => obj is Family other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(typeof(Family), _map.GetHashCode());
        #endregion

        public Person? Leader {
            get { return _map.GetObject<Person>(nameof(Leader)); }
            set { _map.SetObject<Person>(nameof(Leader), value); }
        }

        //public Person[] Members
        //{
        //    get { return _map.GetArray(nameof(Members), ""); }
        //    set { _map.SetArray(nameof(Members), value); }
        //}
    }
}