using System;
using System.Collections.Generic;
using System.IO;

namespace DynaText
{
    public sealed class DynaTextVec : IEmitText, IEquatable<DynaTextVec>
    {
        private readonly List<object?> _values = new List<object?>();

        public void Add(object? value) => _values.Add(value);

        public T[] Get<T>(T defaultValue)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < _values.Count; i++)
            {
                var value = _values[i];
                if (value is null)
                {
                    result.Add(defaultValue);
                }
                else if (value is T tValue)
                {
                    result.Add(tValue);
                }
                else
                {
                    throw new InvalidCastException($"Cannot cast value[{i}] ({value}) from type {value.GetType().Name} to type {typeof(T).Name}.");
                }
            }
            return result.ToArray();
        }

        public string EmitText()
        {
            using var writer = new StringWriter();
            Emit(writer, 0);
            return writer.ToString();
        }

        public bool Emit(TextWriter writer, int indent)
        {
            writer.Write(LexChar.LeftSquare);
            indent += 4;
            int emitted = 0;
            foreach (var value in _values)
            {
                if (emitted > 0) writer.Write(LexChar.Comma);
                writer.WriteLine();
                writer.Write(new string(' ', indent));
                writer.EmitValue(indent, value);
                emitted++;
            }
            writer.WriteLine();
            writer.Write(new string(' ', indent - 4));
            writer.Write(LexChar.RightSquare);
            return true;
        }

        public bool Equals(DynaTextVec? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other._values.Count != _values.Count) return false;
            for (int i = 0; i < _values.Count; i++)
            {
                if (!Equals(other._values[i], _values[i])) return false;
            }
            return true;
        }
        public override bool Equals(object? obj) => obj is DynaTextVec other && Equals(other);
        public override int GetHashCode()
        {
            HashCode result = new HashCode();
            result.Add(_values.Count);
            for (int i = 0; i < _values.Count; i++)
            {
                result.Add(_values[i]);
            }
            return result.ToHashCode();
        }
    }
}