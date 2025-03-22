using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Sandbox.Tests
{
    public readonly struct SourceLine
    {
        private readonly static SourceLine _empty = new SourceLine(0, string.Empty);
        public static SourceLine Empty => _empty;

        public readonly int Line;
        public readonly string Text;

        public SourceLine(int line, string text)
        {
            Line = line;
            Text = text;
        }

        public override string ToString() => Text;
    }

    public enum TokenKind
    {
        None,
        Whitespace,
        String,
        Number,
        Identifier,
        LeftCurly,
        RightCurly,
        Comma,
        Equals,
        LeftSquare,
        RightSquare,
        // todo more special chars
        Error,
    }

    internal readonly struct SourceToken
    {
        public readonly TokenKind Kind;
        public readonly SourceLine Source;
        public readonly int Offset;
        public readonly int Length;
        public readonly string? Error;

        public SourceToken(TokenKind kind, SourceLine source, int offset, int length, string? error = null)
        {
            Kind = kind;
            Source = source;
            Offset = offset;
            Length = length;
            Error = error;
        }

        public string StringValue => Source.Text.Substring(Offset, Length);

        public SourceToken Extend() => new SourceToken(Kind, Source, Offset, Length + 1, null);

        public override string ToString()
        {
            return Kind switch
            {
                TokenKind.Whitespace => " ",
                _ => Source.Text.Substring(Offset, Length),
            };
        }
    }

    internal interface ITextable
    {
        bool Load(TextReader reader);
        void Emit(TextWriter writer, int indent);
    }
    internal static class TextableExtensions
    {
        private static Dictionary<char, string> _map = new Dictionary<char, string>()
        {
            // ch   code    // symbol
            ['%'] = "pc",   // percent
            [';'] = "sc",   // semi-colon
            ['"'] = "dq",   // double-quote
            ['\\'] = "bs",  // back-slash
            ['='] = "eq",   // equals
            [','] = "cm",   // comma
            ['('] = "lp",   // left-paren
            [')'] = "rp",   // right-paren
            ['{'] = "lc",   // left-curly
            ['}'] = "rc",   // right-curly
            ['['] = "ls",   // left-square
            [']'] = "rs",   // left-square
            ['<'] = "la",   // left-angle
            ['>'] = "ra",   // left-angle
        };
        private static ImmutableDictionary<char, string> BuildCharToCodeMap()
        {
            return ImmutableDictionary<char, string>.Empty.AddRange(_map);
        }
        private static ImmutableDictionary<string, char> BuildCodeToCharMap()
        {
            return ImmutableDictionary<string, char>.Empty.AddRange(_map.Select(kvp => new KeyValuePair<string, char>(kvp.Value, kvp.Key)));
        }
        private static readonly ImmutableDictionary<char, string> escapeCharToCode = BuildCharToCodeMap();
        private static readonly ImmutableDictionary<string, char> escapeCodeToChar = BuildCodeToCharMap();
        /// <summary>
        /// Escapes special chars as %xx;
        /// </summary>
        public static string Escaped(this string value)
        {
            ReadOnlySpan<char> span = value.AsSpan();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < span.Length; i++)
            {
                char ch = span[i];
                if (escapeCharToCode.TryGetValue(ch, out string? code))
                {
                    result.Append($"%{code};");
                }
                else
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }
        public static string UnEscape(this string value)
        {
            ReadOnlySpan<char> span = value.AsSpan();

            // shortcuts
            if (span.IndexOf('%') < 0) return value;

            StringBuilder result = new StringBuilder();
            int pos = 0;
            while (pos < span.Length)
            {
                char ch = span[pos];
                if (ch == '%')
                {
                    if (pos + 3 < span.Length && span[pos + 3] == ';')
                    {
                        // ok
                        char ch1 = span[pos + 1];
                        char ch2 = span[pos + 2];
                        string code = new string([ch1,ch2]);
                        if (escapeCodeToChar.TryGetValue(code, out char decoded))
                        {
                            result.Append(decoded);
                        }
                        else
                        {
                            throw new InvalidDataException($"Invalid escape code '{code}' at position {pos} in '{value}'");
                        }
                        // next
                        pos = pos + 4;
                    }
                    else
                    {
                        // bad format
                        throw new InvalidDataException($"Invalid escape code at position {pos} in '{value}'");
                    }
                }
                else
                {
                    result.Append(ch);
                    //next
                    pos++;
                }
            }
            return result.ToString();
        }

        public static IEnumerable<SourceToken> ReadAllTokens(this TextReader reader)
        {
            string? text;
            int line = 0;
            while ((text = reader.ReadLine()) is not null)
            {
                var sourceLine = new SourceLine(line, text);
                foreach(var token in sourceLine.ReadLineTokens())
                {
                    yield return token;
                }
                line++;
            }
        }

        private static IEnumerable<SourceToken> ReadLineTokens(this SourceLine sourceLine)
        {
            SourceToken token = default;
            int offset = -1;
            foreach (char ch in sourceLine.Text)
            {
                offset++;

                bool consumed = false;
                while (!consumed)
                {
                    consumed = true;
                    if (token.Kind == TokenKind.None)
                    {
                        if (char.IsWhiteSpace(ch))
                        {
                            // start of whitespace
                            token = new SourceToken(TokenKind.Whitespace, sourceLine, offset, 1);
                        }
                        else if (char.IsDigit(ch))
                        {
                            // start of number
                            token = new SourceToken(TokenKind.Number, sourceLine, offset, 1);
                        }
                        else if (char.IsLetter(ch) || ch == '_')
                        {
                            // start of identifier
                            token = new SourceToken(TokenKind.Identifier, sourceLine, offset, 1);
                        }
                        else if (ch == '"')
                        {
                            // start of string
                            token = new SourceToken(TokenKind.String, sourceLine, offset + 1, 0);
                        }
                        else
                        {
                            yield return ch switch
                            {
                                '{' => new SourceToken(TokenKind.LeftCurly, sourceLine, offset, 1),
                                '}' => new SourceToken(TokenKind.RightCurly, sourceLine, offset, 1),
                                ',' => new SourceToken(TokenKind.Comma, sourceLine, offset, 1),
                                '[' => new SourceToken(TokenKind.LeftSquare, sourceLine, offset, 1),
                                ']' => new SourceToken(TokenKind.RightSquare, sourceLine, offset, 1),
                                '=' => new SourceToken(TokenKind.Equals, sourceLine, offset, 1),
                                _ => new SourceToken(TokenKind.Error, sourceLine, offset, 1, "Unexpected character"),
                            };
                        }
                    }
                    else if (token.Kind == TokenKind.Whitespace)
                    {
                        // try consume whitespace
                        if (char.IsWhiteSpace(ch))
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.Number)
                    {
                        // try consume number
                        if (char.IsDigit(ch) || ch == '.')
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.Identifier)
                    {
                        // try consume identifier
                        if (char.IsLetterOrDigit(ch) || ch == '_')
                        {
                            token = token.Extend();
                        }
                        else
                        {
                            yield return token;
                            token = default;
                            consumed = false;
                        }
                    }
                    else if (token.Kind == TokenKind.String)
                    {
                        // try consume string
                        if (ch == '"')
                        {
                            // end of string
                            yield return token;
                            token = default;
                        }
                        else
                        {
                            token = token.Extend();
                        }
                    }
                    else
                    {
                        yield return new SourceToken(TokenKind.Error, sourceLine, offset, 1, "Unexpected state");
                        token = default;
                    }
                } // while !consumed

            } // foreach ch

            // emit remaining token
            if (token.Kind == TokenKind.String)
            {
                // non-terminated string!
                yield return new SourceToken(TokenKind.Error, sourceLine, offset, 1, "Unterminated string");
            }
            else if (token.Kind != TokenKind.None)
            {
                yield return token;
            }
        }

        public static void EmitField(this TextWriter writer, int indent, IField field)
        {
            writer.WriteLine();
            writer.Write(new string(' ', indent));
            writer.Write(field.Name);
            writer.Write(" = ");
            field.WriteValue(writer, indent);
        }

        public static void EmitFields(this TextWriter writer, int indent, params IField[] fields)
        {
            writer.Write('{');
            indent += 4;
            int emitted = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsDefaultValue)
                {
                    if (emitted > 0) writer.Write(',');
                    writer.EmitField(indent, field);
                    emitted++;
                }
            }
            indent -= 4;
            writer.WriteLine();
            writer.Write(new string(' ', indent));
            writer.Write('}');
        }

        private static bool TryConsumeOneToken(ReadOnlySpan<SourceToken> tokens, TokenKind tokenKind) => tokens.Length > 0 && tokens[0].Kind == tokenKind;

        private static bool TryConsumeOneToken(ReadOnlySpan<SourceToken> tokens, TokenKind tokenKind, out SourceToken token)
        {
            token = default;
            if (tokens.Length == 0 || tokens[0].Kind != tokenKind) return false;
            token = tokens[0];
            return true;
        }

        public static bool TryConsumeScalar(ReadOnlySpan<SourceToken> remaining, out int result, Func<string, bool> valueHandler)
        {
            result = 0;

            // try consume string value
            if (TryConsumeOneToken(remaining, TokenKind.String, out SourceToken token1) && valueHandler(token1.StringValue))
            {
                result = 1;
                return true;
            }

            // try consume number value
            if (TryConsumeOneToken(remaining, TokenKind.Number, out SourceToken token2) && valueHandler(token2.StringValue))
            {
                result = 1;
                return true;
            }

            // try consume identifier value e.g. true or false
            if (TryConsumeOneToken(remaining, TokenKind.Identifier, out SourceToken token3) && valueHandler(token3.StringValue))
            {
                result = 1;
                return true;
            }

            return false;
        }

        public static bool TryConsumeVector(ReadOnlySpan<SourceToken> remaining, out int result, Func<string, bool> valueHandler)
        {
            result = 0;
            int tokensConsumed = 0;

            // try consume begin vector
            if (!TryConsumeOneToken(remaining, TokenKind.LeftSquare)) return false;
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            int fieldsConsumed = 0;
            while (true)
            {
                if (remaining.Length == 0) return false;

                // try consume close vector
                if (TryConsumeOneToken(remaining, TokenKind.RightSquare))
                {
                    remaining = remaining.Slice(1);
                    tokensConsumed += 1;
                    result = tokensConsumed;
                    return true;
                }

                // try consume value separator
                if (fieldsConsumed > 0)
                {
                    if (!TryConsumeOneToken(remaining, TokenKind.Comma)) return false;
                    remaining = remaining.Slice(1);
                    tokensConsumed += 1;
                }

                // try consume scalar
                if (!TryConsumeScalar(remaining, out int consumed2, valueHandler)) return false;
                remaining = remaining.Slice(consumed2);
                tokensConsumed += consumed2;
                fieldsConsumed++;
            }
        }

        private static bool TryConsumeField(ReadOnlySpan<SourceToken> remaining, Dictionary<string, IField> fieldMap, out int result)
        {
            result = 0;
            int tokensConsumed = 0;

            // try consume field
            if (!TryConsumeOneToken(remaining, TokenKind.Identifier, out SourceToken token)) return false;
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            // check field name
            string fieldName = token.StringValue;
            if (!fieldMap.TryGetValue(fieldName, out IField? field)) 
                throw new InvalidDataException($"Unknown field name '{fieldName}' at position {token.Offset} on line {token.Source.Line}.");

            // try consume equals
            if (!TryConsumeOneToken(remaining, TokenKind.Equals)) return false;
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            // try consume value as vector
            if (TryConsumeVector(remaining, out int consumed2, field.ValueParser))
            {
                remaining = remaining.Slice(consumed2);
                tokensConsumed += consumed2;
                result = tokensConsumed;
                return true;
            }

            // try consume value as scalar
            if (TryConsumeScalar(remaining, out int consumed3, field.ValueParser))
            {
                remaining = remaining.Slice(consumed3);
                tokensConsumed += consumed3;
                result = tokensConsumed;
                return true;
            }

            return false;
        }

        private static bool TryConsumeFields(ReadOnlySpan<SourceToken> remaining, Dictionary<string, IField> fieldMap, out int consumed)
        {
            consumed = 0;
            int position = 0;

            // consume begin field group
            if (!TryConsumeOneToken(remaining, TokenKind.LeftCurly)) return false;
            remaining = remaining.Slice(1);
            position += 1;

            // consume fields
            int fieldsConsumed = 0;
            while (true)
            {
                if (remaining.Length == 0) return false;

                // try consume close field group
                if (TryConsumeOneToken(remaining, TokenKind.RightCurly))
                {
                    remaining = remaining.Slice(1);
                    position += 1;
                    consumed = position;
                    return true;
                }

                // try consume field separator
                if (fieldsConsumed > 0)
                {
                    if (!TryConsumeOneToken(remaining, TokenKind.Comma)) return false;
                    remaining = remaining.Slice(1);
                    position += 1;
                }

                // try consume field
                if (!TryConsumeField(remaining, fieldMap, out int consumed2)) return false;
                remaining = remaining.Slice(consumed2);
                position += consumed2;
                fieldsConsumed++;
            }
        }

        public static bool LoadFields(this TextReader reader, params IField[] fields)
        {
            var fieldMap = new Dictionary<string, IField>();
            foreach (var field in fields)
            {
                fieldMap[field.Name] = field;
            }

            // ignore whitespace
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().Where(t => t.Kind != TokenKind.Whitespace).ToArray().AsSpan();

            // consume token stream
            return TryConsumeFields(tokens, fieldMap, out int _);
        }

        public static string ToText(this ITextable source)
        {
            using var sw = new StringWriter();
            source.Emit(sw, 0);
            return sw.ToString();
        }
    }
    internal interface IFieldMeta<T>
    {
        T DefaultValue { get; }
        bool IsDefaultValue(T value);
        string FormatValue(T value);
        bool TryParseValue(string input, out T result);
    }
    internal sealed class FieldMetaInt : IFieldMeta<int>
    {
        public int DefaultValue => default;
        public string FormatValue(int value) => value.ToString();
        public bool IsDefaultValue(int value) => value == default;
        public bool TryParseValue(string input, out int result) => int.TryParse(input, out result);
    }
    internal sealed class FieldMetaBool : IFieldMeta<bool>
    {
        public bool DefaultValue => default;
        public string FormatValue(bool value) => value.ToString().ToLower();
        public bool IsDefaultValue(bool value) => value == default;
        public bool TryParseValue(string input, out bool result) => bool.TryParse(input, out result);
    }
    internal sealed class FieldMetaByte : IFieldMeta<byte>
    {
        public byte DefaultValue => default;
        public string FormatValue(byte value) => value.ToString().ToLower();
        public bool IsDefaultValue(byte value) => value == default;
        public bool TryParseValue(string input, out byte result) => byte.TryParse(input, out result);
    }
    internal sealed class FieldMetaString : IFieldMeta<string>
    {
        public string DefaultValue => string.Empty;
        public string FormatValue(string value)
        {
            StringBuilder result = new StringBuilder();
            result.Append('"');
            result.Append(value.Escaped());
            result.Append('"');
            return result.ToString();
        }
        public bool IsDefaultValue(string value) => value == string.Empty;
        public bool TryParseValue(string input, out string result)
        {
            result = input.UnEscape();
            return true;
        }
    }
    internal static class MetaHelper
    {
        public static IFieldMeta<T> GetMeta<T>()
        {
            if (typeof(T) == typeof(int))
                return (IFieldMeta<T>)(IFieldMeta<int>)(new FieldMetaInt());
            else if (typeof(T) == typeof(bool))
                return (IFieldMeta<T>)(IFieldMeta<bool>)(new FieldMetaBool());
            else if (typeof(T) == typeof(string))
                return (IFieldMeta<T>)(IFieldMeta<string>)(new FieldMetaString());
            else if (typeof(T) == typeof(byte))
                return (IFieldMeta<T>)(IFieldMeta<byte>)(new FieldMetaByte());
            else
                throw new NotSupportedException($"IFieldMeta<{typeof(T).Name}>");
        }
    }
    internal interface IField
    {
        string Name { get; }
        Type Type { get; }
        bool IsDefaultValue { get; }
        void WriteValue(TextWriter writer, int indent);
        bool ValueParser(string value);
    }
    internal abstract class AnyType<T>
    {
        protected readonly IFieldMeta<T> _meta;
        protected readonly string _name;
        protected readonly bool _nullable;

        protected AnyType(IFieldMeta<T> meta, string name, bool nullable)
        {
            _meta = meta;
            _name = name;
            _nullable = nullable;
        }

        protected AnyType(string name, bool nullable)
        {
            _meta = MetaHelper.GetMeta<T>();
            _name = name;
            _nullable = nullable;
        }

        public string Name => _name;
        public Type Type => typeof(T);
    }
    internal sealed class ScalarValType<T> : AnyType<T>, IField, IEquatable<ScalarValType<T>> where T : struct, IEquatable<T>
    {
        private T? _value;

        public ScalarValType(string name, bool nullable = false) : base(name, nullable)
        {
            _value = nullable ? null : default(T);
        }

        public bool IsDefaultValue => _nullable ? _value is null : _value is null ? false : _meta.IsDefaultValue(_value.Value);
        public T? NullableValue
        {
            get { return _value; }
            set { _value = value; }
        }
        public T Value
        {
            get { return _value ?? default(T); }
            set { _value = value; }
        }

        public bool ValueParser(string input)
        {
            if (!_meta.TryParseValue(input, out T result)) return false;
            _value = result;
            return true;
        }

        private static bool ValuesAreEqual(T? left, T? right)
        {
            if (left is null) return (right is null);
            return (right is null) ? false : left.Value.Equals(right.Value);
        }

        public bool Equals(ScalarValType<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!ValuesAreEqual(other._value, _value)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is ScalarValType<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_name, _value);
        public override string ToString() => $"{_name}<{typeof(T).Name}>={_value}";

        public void WriteValue(TextWriter writer, int indent)
        {
            string formatted = _value is null ? "nul" : _meta.FormatValue(_value.Value);
            writer.Write(formatted);
        }
    }
    internal sealed class ScalarRefType<T> : AnyType<T>, IField, IEquatable<ScalarRefType<T>> where T : class, IEquatable<T>
    {
        private T? _value;

        public ScalarRefType(string name, bool nullable = false) : base(name, nullable)
        {
            _value = nullable ? null : _meta.DefaultValue;
        }

        public bool IsDefaultValue => _nullable ? _value is null : _value is null ? false : _meta.IsDefaultValue(_value);
        public T? NullableValue
        {
            get { return _value; }
            set { _value = value; }
        }
        public T Value
        {
            get { return _value ?? _meta.DefaultValue; }
            set { _value = value; }
        }

        public void WriteValue(TextWriter writer, int indent)
        {
            string formatted = _value is null ? "nul" : _meta.FormatValue(_value);
            writer.Write(formatted);
        }

        public bool ValueParser(string value)
        {
            return _meta.TryParseValue(value, out _value);
        }

        private static bool ValuesAreEqual(T? left, T? right)
        {
            if (left is null) return (right is null);
            return (right is null) ? false : left.Equals(right);
        }

        public bool Equals(ScalarRefType<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!ValuesAreEqual(other._value, _value)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is ScalarRefType<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_name, _value);
        public override string ToString() => $"{_name}<{typeof(T).Name}>={_value}";
    }

    public readonly struct ValueArray<T> : IEquatable<ValueArray<T>> where T : IEquatable<T>
    {
        private static ValueArray<T> _empty = new ValueArray<T>(Array.Empty<T>());
        public static ValueArray<T> Empty => _empty;

        public readonly T[] Values;
        public ValueArray(T[] values) => Values = values;
        public int Length => Values.Length;
        public bool Equals(ValueArray<T> other) => other.Values.AsSpan().SequenceEqual(Values.AsSpan());
        public override bool Equals(object? obj) => obj is ValueArray<T> other && Equals(other);
        public override int GetHashCode()
        {
            HashCode result = new HashCode();
            result.Add(typeof(T));
            var values = Values.AsSpan();
            result.Add(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                result.Add(values[i]);
            }
            return result.ToHashCode();
        }
    }

    internal enum MyEnum1 : byte
    {
        None = 0,
        First = 1,
        Other = 3,
        Final = 9,
    }

    internal enum MyEnum2 : int
    {
        First = 1,
        Other = 3,
        Final = 9,
    }

    internal readonly struct EnumValue<T> : IEquatable<EnumValue<T>> where T :struct, Enum
    {
        public readonly T Value;
        public EnumValue(T value)
        {
            Value = value;
        }

        public bool Equals(EnumValue<T> other)
        {
            return Enum.Equals(other.Value, Value);
        }
    }

    internal sealed class Member : ITextable, IEquatable<Member>
    {
        private readonly ScalarValType<int> _id = new ScalarValType<int>(nameof(Id), false);
        public int Id { get => _id.Value; set => _id.Value = value; }

        private readonly ScalarRefType<string> _name = new ScalarRefType<string>(nameof(Name));
        public string Name { get => _name.Value; set => _name.Value = value; }

        private readonly ScalarRefType<string> _type = new ScalarRefType<string>(nameof(Type));
        public string Type { get => _type.Value; set => _type.Value = value; }

        private readonly ScalarValType<bool> _nullable = new ScalarValType<bool>(nameof(Nullable), true);
        public bool? Nullable { get => _nullable.NullableValue; set => _nullable.NullableValue = value; }

        private readonly ScalarRefType<string> _desc = new ScalarRefType<string>(nameof(Description), true);
        public string? Description { get => _desc.NullableValue; set => _desc.NullableValue = value; }

        private readonly ScalarValType<byte> _kind1 = new ScalarValType<byte>(nameof(Kind1), false);
        public MyEnum1 Kind1 { get => (MyEnum1)_kind1.Value; set => _kind1.Value = (byte)value; }

        private readonly ScalarValType<int> _kind2 = new ScalarValType<int>(nameof(Kind2), true);
        public MyEnum2? Kind2 { get => (MyEnum2?)_kind2.NullableValue; set => _kind2.NullableValue = (int?)value; }

        //private readonly ValType<ValueArray<int>> _dims = new ValType<ValueArray<int>>(nameof(Dims), ValueArray<int>.Empty, (va) => (va?.Length ?? 0) == 0, xxx);
        //public int[] Dims
        //{
        //    get => (_dims.Value ?? ValueArray<int>.Empty).Values;
        //    set => _dims.Value = new ValueArray<int>(value);
        //}
        //public int Rank => _dims.Value?.Length ?? 0;

        public void Emit(TextWriter writer, int indent) => writer.EmitFields(indent, _id, _name, _type, _nullable, _desc, _kind1, _kind2);
        public bool Load(TextReader reader) => reader.LoadFields(_id, _name, _type, _nullable, _desc, _kind1, _kind2);

        public bool Equals(Member? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._id.Equals(_id)) return false;
            if (!other._name.Equals(_name)) return false;
            if (!other._type.Equals(_type)) return false;
            if (!other._nullable.Equals(_nullable)) return false;
            if (!other._desc.Equals(_desc)) return false;
            if (!other._kind1.Equals(_kind1)) return false;
            if (!other._kind2.Equals(_kind2)) return false;
            //if (!other._dims.Equals(_dims)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Member other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_id, _name, _type, _nullable, _desc, _kind1, _kind2);

    }
    public class TextableTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("abc.def", "abc.def")]
        [InlineData("10%", "10%pc;")]
        [InlineData("{abc}", "%lc;abc%rc;")]
        [InlineData("(abc)", "%lp;abc%rp;")]
        [InlineData("[abc]", "%ls;abc%rs;")]
        [InlineData("<abc>", "%la;abc%ra;")]
        [InlineData("abc\\def", "abc%bs;def")]
        [InlineData("abc\"def", "abc%dq;def")]
        public void Lexer0_StringEscaping(string value, string expectedEncoding)
        {
            // encode
            string encoded = value.Escaped();
            encoded.ShouldBe(expectedEncoding);

            // decode
            string decoded = encoded.UnEscape();
            decoded.ShouldBe(value);
        }

        [Fact]
        public async Task EmitLoad0_EmptyObject()
        {
            Member orig = new Member();

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task EmitLoad1_SimpleObject()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
            };

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]

        public async Task EmitLoad2_OptRefTypeA()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = null,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task EmitLoad3_OptRefTypeB()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = "abc",
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task EmitLoad4_OptValTypeA()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task EmitLoad5_OptValTypeB()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = true,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task EmitLoad6_ReqEnumType()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(MyEnum1).FullName!,
                //Nullable = true,
                Kind1 = MyEnum1.First,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);

            copy.Kind1.ShouldBe(MyEnum1.First);
        }

        [Fact]
        public async Task EmitLoad7_OptEnumType()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(MyEnum2).FullName!,
                //Nullable = true,
                //Kind2 = MyEnum2.First,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);

            copy.Kind2.HasValue.ShouldBeFalse();
            copy.Kind2.ShouldBeNull();
        }

        [Fact]
        public async Task EmitLoad8_VectorValType()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                //Dims = new int[] { 2, 2, 2 },
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Theory]
        [InlineData(1, """{Id=123,Name="Field1",Type="System.String"}""")] // original encoding
        [InlineData(2, """{Id=123,Type="System.String",Name="Field1"}""")] // field order re-arranged
        [InlineData(3, """ {Id=123,Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(4, """{ Id=123,Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(5, """{Id =123,Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(6, """{Id= 123,Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(7, """{Id=123 ,Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(8, """{Id=123, Name="Field1",Type="System.String"}""")] // random whitespace
        [InlineData(9, """{Id=123,Name="Field1",Type="System.String" }""")] // random whitespace
        public void Parser1_Success(int _, string encoded)
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };

            Member copy = new Member();
            using var reader = new StringReader(encoded);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Parser2_Success_IndentedInput()
        {
            const string encoded =
                """
                {
                    Id   = 123
                    ,
                    Name = "Field1"
                    ,
                    Type = "System.String"
                }
                """;

            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };

            Member copy = new Member();
            using var reader = new StringReader(encoded);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Parser3_Failure_UnknownField()
        {
            string encoded =
                """
                {
                    Id=123,
                    Name="Field1",
                    Type="System.String",
                    FieldX="abcdef"
                }
                """;
            var ex = Assert.Throws<InvalidDataException>(() =>
            {
                Member copy = new Member();
                using var reader = new StringReader(encoded);
                bool loaded = copy.Load(reader);
                loaded.ShouldBeTrue();
            });
            ex.Message.ShouldBe("Unknown field name 'FieldX' at position 4 on line 4.");
        }

        [Fact]
        public async Task Load5_Scalar()
        {
            Member orig = new Member()
            {
                Id = 123,
            };

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Load6_Vector()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                //Dims = new int[] { 2, 2, 2 },
            };

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Member copy = new Member();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Theory]
        [InlineData(0, """{""", TokenKind.LeftCurly)]
        [InlineData(1, """}""", TokenKind.RightCurly)]
        [InlineData(2, """ """, TokenKind.Whitespace)]
        [InlineData(3, """abc""", TokenKind.Identifier)]
        [InlineData(4, """123""", TokenKind.Number)]
        [InlineData(5, """=""", TokenKind.Equals)]
        public void Lexer1_Success(int _, string encoded, TokenKind expectedTokenKind)
        {
            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThanOrEqualTo(1);
            tokens[0].Kind.ShouldBe(expectedTokenKind);
        }

        [Fact]
        public void Lexer2_Failure_UnexpectedChar()
        {
            string encoded =
                """
                .
                """;

            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThan(0);
            var token = tokens.Last();
            token.Kind.ShouldBe(TokenKind.Error);
            token.Error.ShouldBe("Unexpected character");
        }

        [Fact]
        public void Lexer3_Failure_UnterminatedString()
        {
            string encoded =
                """
                "abc
                """;

            using var reader = new StringReader(encoded);
            var tokens = reader.ReadAllTokens().ToList();
            tokens.Count.ShouldBeGreaterThan(0);
            var token = tokens.Last();
            token.Kind.ShouldBe(TokenKind.Error);
            token.Error.ShouldBe("Unterminated string");
        }

    }
}