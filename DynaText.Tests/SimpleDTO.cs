using System;
using System.IO;
using System.Linq;

namespace DynaText.Tests
{
    internal class SimpleDTO : IEmitText, ILoadText, IEquatable<SimpleDTO>
    {
        private DynaTextMap _map = new DynaTextMap();
        public bool Emit(TextWriter writer, int indent) => _map.Emit(writer, indent);
        public void LoadFrom(string text)
        {
            // todo simplify
            // _map = DynaTextMap.LoadFrom(text);
            using var reader = new StringReader(text);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            if (result.IsError) throw new InvalidDataException(result.Message);
            _map = result.Output as DynaTextMap ?? throw new InvalidDataException();
        }

        public bool Equals(SimpleDTO? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _map.Equals(other._map);
        }
        public override bool Equals(object? obj) => obj is SimpleDTO other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(typeof(SimpleDTO), _map.GetHashCode());

        public int Id { get { return _map.Get(nameof(Id), 0); } set { _map.Set(nameof(Id), value); } }
        public string Surname { get { return _map.Get(nameof(Surname), ""); } set { _map.Set(nameof(Surname), value); } }
        public string? Nickname { get { return _map.Get<string?>(nameof(Nickname), null); } set { _map.Set(nameof(Nickname), value); } }

    }
}