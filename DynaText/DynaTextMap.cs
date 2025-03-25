using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DynaText
{
    public sealed class DynaTextMap : IEmitText, IEquatable<DynaTextMap>
    {
        public static DynaTextMap LoadFrom(string text)
        {
            using var reader = new StringReader(text);
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().ToArray().AsSpan();
            ParseResult result = tokens.ParseTokens();
            if (result.IsError) throw new InvalidDataException(result.Message);
            return result.Output as DynaTextMap ?? throw new InvalidDataException();
        }

        private Dictionary<string, object?> _values = new Dictionary<string, object?>();

        public void Add(string key, object? value) => _values[key] = value;

        public T Get<T>(string key, T defaultValue)
        {
            if (!_values.TryGetValue(key, out var obj)) return defaultValue;
            if (obj is null) return defaultValue;
            if (obj is T tValue) return tValue;
            throw new InvalidCastException($"Cannot cast value ({obj}) from type {obj.GetType().Name} to type {typeof(T).Name}.");
        }

        public void Set<T>(string key, T value)
        {
            _values[key] = value;
        }

        public T?[] GetArray<T>(string key, T? defaultValue)
        {
            if (!_values.TryGetValue(key, out var obj)) return [];
            return obj switch
            {
                null => [],
                DynaTextVec vector => vector.GetValues(defaultValue),
                _ => throw new InvalidCastException($"Cannot cast value ({obj}) from type {obj.GetType().Name} to type {typeof(T).Name}.")
            };
        }

        public void SetArray<T>(string key, T?[] values)
        {
            var vector = new DynaTextVec();
            for (int i = 0; i < values.Length; i++)
            {
                vector.Add(values[i]);
            }
            _values[key] = vector;
        }

        public T? GetObject<T>(string key) where T : class, IDynaText, new()
        {
            if (!_values.TryGetValue(key, out var obj)) return null;
            return obj switch
            {
                null => null,
                DynaTextMap map => map.ToObject<T>(),
                _ => throw new InvalidCastException($"Cannot cast value ({obj}) from type {obj.GetType().Name} to type {typeof(T).Name}.")
            };
        }

        public void SetObject<T>(string key, T? value) where T : class, IDynaText
        {
            _values[key] = value?.GetMap();
        }

        public T?[] GetVector<T>(string key, T? defaultValue) where T : class, IDynaText, new()
        {
            if (!_values.TryGetValue(key, out var obj)) return [];
            return obj switch
            {
                null => [],
                DynaTextVec vector => vector.GetObjects<T>(defaultValue),
                _ => throw new InvalidCastException($"Cannot cast value ({obj}) from type {obj.GetType().Name} to type {typeof(T).Name}.")
            };
        }

        public void SetVector<T>(string key, T?[] values) where T : class, IDynaText
        {
            var vector = new DynaTextVec();
            for (int i = 0; i < values.Length; i++)
            {
                vector.Add(values[i]?.GetMap());
            }
            _values[key] = vector;
        }

        public string EmitText()
        {
            using var writer = new StringWriter();
            Emit(writer, 0);
            return writer.ToString();
        }


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