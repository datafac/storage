using System;
using System.Collections.Generic;
using System.IO;

namespace DynaText
{
    public sealed class DynaTextMap : IEmitText, IEquatable<DynaTextMap>
    {
        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();

        public void Add(string key, object? value) => _values[key] = value;

        public bool Emit(TextWriter writer, int indent)
        {
            writer.Write(LexChar.LeftCurly);
            indent += 4;
            int emitted = 0;
            foreach (var kvp in _values)
            {
                if (emitted > 0) writer.Write(LexChar.Comma);
                writer.WriteLine();
                writer.Write(new string(' ', indent));
                writer.Write(kvp.Key);
                writer.Write(" = ");
                writer.EmitValue(indent, kvp.Value);
                emitted++;
            }
            writer.WriteLine();
            writer.Write(new string(' ', indent - 4));
            writer.Write(LexChar.RightCurly);
            return true;
        }

        public bool Equals(DynaTextMap? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other._values.Count != _values.Count) return false;
            foreach (var kvp in _values)
            {
                if (!other._values.TryGetValue(kvp.Key, out object? otherValue)) return false;
                if (!Equals(otherValue, kvp.Value)) return false;
            }
            return true;
        }
        public override bool Equals(object? obj) => obj is DynaTextMap other && Equals(other);
        public override int GetHashCode()
        {
            HashCode result = new HashCode();
            result.Add(_values.Count);
            foreach (var kvp in _values)
            {
                result.Add(kvp.Key);
                result.Add(kvp.Value);
            }
            return result.ToHashCode();
        }
    }
}