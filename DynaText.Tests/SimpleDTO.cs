using System;
using System.IO;

namespace DynaText.Tests
{
    internal class SimpleDTO : IEmitText, ILoadText, IEquatable<SimpleDTO>
    {
        #region boilerplate
        private DynaTextMap _map = new DynaTextMap();
        public bool Emit(TextWriter writer, int indent) => _map.Emit(writer, indent);
        public void LoadFrom(string text) => _map = DynaTextMap.LoadFrom(text);
        public bool Equals(SimpleDTO? other) => other is null ? false : ReferenceEquals(this, other) ? true : _map.Equals(other._map);
        public override bool Equals(object? obj) => obj is SimpleDTO other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(typeof(SimpleDTO), _map.GetHashCode());
        #endregion

        public int Id { get { return _map.Get(nameof(Id), 0); } set { _map.Set(nameof(Id), value); } }
        public string Surname { get { return _map.Get(nameof(Surname), ""); } set { _map.Set(nameof(Surname), value); } }
        public string? Nickname { get { return _map.Get<string?>(nameof(Nickname), null); } set { _map.Set(nameof(Nickname), value); } }
        public ushort? YOB { get { return _map.Get<ushort?>(nameof(YOB), null); } set { _map.Set(nameof(YOB), value); } }
        public string[] OtherNames
        {
            get { return _map.GetArray(nameof(OtherNames), ""); }
            set { _map.SetArray(nameof(OtherNames), value); }
        }
    }
}