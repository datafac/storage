using System;
using System.Collections.Generic;
using System.IO;

namespace DynaText
{
    public readonly struct ParseResult
    {
        public readonly int Consumed;
        public readonly object? Output;
        public readonly string? Message;
        public ParseResult(string message)
        {
            Consumed = 0;
            Message = message;
            Output = null;
        }
        public ParseResult(int consumed, object? output)
        {
            Consumed = consumed;
            Output = output;
            Message = null;
        }

        public bool IsError => Message is not null;
    }
    internal interface IEmitText
    {
        bool Emit(TextWriter writer, int indent);
    }
    public sealed class DynaTextVec : IEmitText, IEquatable<DynaTextVec>
    {
        private readonly List<object?> _values = new List<object?>();

        public void Add(object? value) => _values.Add(value);

        #region IEmitText
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
        #endregion

        #region IEquatable
        public bool Equals(DynaTextVec? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other._values.Count != _values.Count) return false;
            for (int i = 0; i < _values.Count; i++)
            {
                if (!Equals(other._values[i], _values[i])) return false;
            }
            foreach (var kvp in _values)
            {
            }
            return true;
        }
        #endregion
    }
    public sealed class DynaTextMap : IEmitText, IEquatable<DynaTextMap>
    {
        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();

        public void Add(string key, object? value) => _values[key] = value;

        #region IEmitText
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
        #endregion

        #region IEquatable
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
        #endregion
    }
}