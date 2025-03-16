using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using Xunit;

namespace Sandbox.Tests
{
    internal interface ITextable
    {
        void Emit(StringBuilder builder);
        bool Load(string source);
    }
    public readonly struct ReadResult
    {
        public static ReadResult Fail() => new ReadResult();
        public static ReadResult Good(int consumed) => new ReadResult(true, consumed);
        public static ReadResult Good(int consumed, object? value) => new ReadResult(true, consumed, true, value);

        public readonly bool Success;
        public readonly int Consumed;
        public readonly bool HasValue;
        public readonly object? Value;

        public bool Failed => !Success;

        private ReadResult(bool success = false, int consumed = 0, bool hasValue = false, object? value = null)
        {
            Success = success;
            Consumed = consumed;
            HasValue = hasValue;
            Value = value;
        }
    }
    internal static class TextableExtensions
    {
        private static Dictionary<char, string> _map = new Dictionary<char, string>()
        {
            // ch   code    // symbol
            ['%'] = "pc",   // percent
            [';'] = "sc",   // semi-colon
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

        public static void EmitValue(this StringBuilder builder, object? value)
        {
            string formatted = value switch
            {
                null => "nul",
                bool log => $"log({log})",
                sbyte i08 => $"i08({i08})",
                short i16 => $"i16({i16})",
                int i32 => $"i32({i32})",
                long i64 => $"i64({i64})",
                float r32 => $"r32({r32.ToString("R")})",
                double r64 => $"r64({r64.ToString("R")})",
                string str => $"str({str.Escaped()})",
                _ => $"unk({(value?.ToString() ?? "").Escaped()})"
            };
            builder.Append(formatted);
        }

        public static void EmitArray(this StringBuilder builder, object? untypedArray)
        {
            switch (untypedArray)
            {
                case ValueArray<bool> array:
                    {
                        builder.Append("log[");
                        for (int i = 0; i< array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value);
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<sbyte> array:
                    {
                        builder.Append("i08[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value);
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<short> array:
                    {
                        builder.Append("i16[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value);
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<int> array:
                    {
                        builder.Append("i32[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value);
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<long> array:
                    {
                        builder.Append("i64[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value);
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<float> array:
                    {
                        builder.Append("r32[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value.ToString("R"));
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<double> array:
                    {
                        builder.Append("r64[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value.ToString("R"));
                        }
                        builder.Append(']');
                    }
                    break;
                case ValueArray<string> array:
                    {
                        builder.Append("str[");
                        for (int i = 0; i < array.Length; i++)
                        {
                            var value = array.Values[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(value.Escaped());
                        }
                        builder.Append(']');
                    }
                    break;
                default:
                    builder.Append($"unk({(untypedArray?.ToString() ?? "").Escaped()})");
                    break;
            }
        }

        public static void EmitField(this StringBuilder builder, IField field)
        {
            builder.Append(field.Name);
            builder.Append('=');
            object? value = field.UntypedValue;
            if(value is null)
            {
                builder.Append("nul");
                return;
            }
            // check if is ValueArray<T>
            Type type = value.GetType();
            if(type.IsGenericType && type.GenericTypeArguments.Length == 1 && type.Name == "ValueArray`1")
            {
                // vector
                EmitArray(builder, value);
            }
            else
            {
                // scalar
                EmitValue(builder, value);
            }
        }

        public static ReadResult ConsumeVector(ReadOnlySpan<char> source, Func<string, bool> valueHandler)
        {
            int position = 0;
            var remaining = source.Slice(position);

            // consume begin char
            var parsed = ConsumeChar(remaining, '[');
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            while (true)
            {
                StringBuilder matched = new StringBuilder();
                while (remaining.Length > 0 && remaining[0] != ']' && remaining[0] != ',')
                {
                    matched.Append(remaining[0]);
                    position++;
                    remaining = source.Slice(position);
                }

                if (!valueHandler(matched.ToString())) return ReadResult.Fail();

                // try consume close char
                parsed = ConsumeChar(remaining, ']');
                if (parsed.Success)
                {
                    position += parsed.Consumed;
                    remaining = source.Slice(position);
                    return ReadResult.Good(position);
                }

                // try consume separator
                parsed = ConsumeChar(remaining, ',');
                if (parsed.Failed) return ReadResult.Fail();
                position += parsed.Consumed;
                remaining = source.Slice(position);
            }
        }

        public static ReadResult ConsumeScalar(ReadOnlySpan<char> source, Func<string, bool> valueHandler)
        {
            int position = 0;
            var remaining = source.Slice(position);
            var parsed = ConsumeGroup(remaining, '(', ')');
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);
            string? encoded = parsed.Value as string;
            if (encoded is not null && valueHandler(encoded))
                return ReadResult.Good(position);
            else
                return ReadResult.Fail();
        }

        public static ReadResult ConsumeGroup(ReadOnlySpan<char> source, char beginGroup, char closeGroup)
        {
            int position = 0;
            var remaining = source.Slice(position);
            StringBuilder matched = new StringBuilder();

            // match begin char
            var parsed = ConsumeChar(remaining, beginGroup);
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            while (remaining.Length > 0 && remaining[0] != closeGroup)
            {
                matched.Append(remaining[0]);
                position++;
                remaining = source.Slice(position);
            }

            // match close char
            if (remaining.Length == 0 || remaining[0] != closeGroup) return ReadResult.Fail();
            position++;
            remaining = source.Slice(position);

            return ReadResult.Good(position, matched.ToString());
        }

        private static object? TryParseValue(string prefix, string unparsed)
        {
            return prefix switch
            {
                "log" => bool.TryParse(unparsed, out bool log) ? log : null,
                "i08" => sbyte.TryParse(unparsed, out sbyte i08) ? i08 : null,
                "i16" => short.TryParse(unparsed, out short i16) ? i16 : null,
                "i32" => int.TryParse(unparsed, out int i32) ? i32 : null,
                "i64" => long.TryParse(unparsed, out long i64) ? i64 : null,
                "r32" => float.TryParse(unparsed, out float r32) ? r32 : null,
                "r64" => double.TryParse(unparsed, out double r64) ? r64 : null,
                "str" => unparsed,
                _ => null,
            };
        }

        public static ReadResult ConsumeAnyValue(ReadOnlySpan<char> source)
        {
            int position = 0;
            var remaining = source.Slice(position);
            StringBuilder matched = new StringBuilder();

            // consume any leading whitespace
            while (remaining.Length > 0 && char.IsWhiteSpace(remaining[0]))
            {
                position++;
                remaining = source.Slice(position);
            }

            // 1st 3 chars must be an encoded value
            if (remaining.Length < 3) return ReadResult.Fail();

            // get prefix
            string prefix = new string([remaining[0], remaining[1], remaining[2]]);
            position += 3;
            remaining = source.Slice(position);

            if (prefix == "nul") return ReadResult.Good(position, null);

            // check prefix
            bool valid = prefix switch
            {
                "log" => true,
                "i08" => true,
                "i16" => true,
                "i32" => true,
                "i64" => true,
                "r32" => true,
                "r64" => true,
                "str" => true,
                _ => false,
            };

            if (!valid) return ReadResult.Fail();

            // try consume vector
            // consume any text grouped by '[' and ']' separated by ','
            List<object> vectorValues = new List<object>();
            var parsed = ConsumeVector(remaining, (string escapedText) =>
            {
                string unparsed = escapedText.UnEscape();
                object? value = TryParseValue(prefix, unparsed);
                if (value is null) return false;
                vectorValues.Add(value);
                return true;
            });
            if (parsed.Success)
            {
                position += parsed.Consumed;
                remaining = source.Slice(position);
                object? value = prefix switch
                {
                    "log" => new ValueArray<bool>(vectorValues.OfType<bool>().ToArray()),
                    "i08" => new ValueArray<sbyte>(vectorValues.OfType<sbyte>().ToArray()),
                    "i16" => new ValueArray<short>(vectorValues.OfType<short>().ToArray()),
                    "i32" => new ValueArray<int>(vectorValues.OfType<int>().ToArray()),
                    "i64" => new ValueArray<long>(vectorValues.OfType<long>().ToArray()),
                    "r32" => new ValueArray<float>(vectorValues.OfType<float>().ToArray()),
                    "r64" => new ValueArray<double>(vectorValues.OfType<double>().ToArray()),
                    "str" => new ValueArray<string>(vectorValues.OfType<string>().ToArray()),
                    _ => false,
                };

                return ReadResult.Good(position, value);
            }

            // else try consume scalar
            object? scalarValue = null;
            parsed = ConsumeScalar(remaining, (string escapedText) =>
            {
                string unparsed = escapedText.UnEscape();
                object? value = TryParseValue(prefix, unparsed);
                if (value is null) return false;
                scalarValue = value;
                return true;
            });
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);
            return scalarValue is null ? ReadResult.Fail() : ReadResult.Good(position, scalarValue);
        }

        public static void EmitFields(this StringBuilder builder, params IField[] fields)
        {
            builder.Append('{');
            int emitted = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsDefault)
                {
                    if (emitted > 0) builder.Append(',');
                    builder.EmitField(field);
                    emitted++;
                }
            }
            builder.Append('}');
        }

        public static ReadResult ConsumeChar(ReadOnlySpan<char> source, char ch)
        {
            int position = 0;
            ReadOnlySpan<char> remaining = source.Slice(position);

            // consume any leading whitespace
            while (remaining.Length > 0 && char.IsWhiteSpace(remaining[0]))
            {
                position++;
                remaining = source.Slice(position);
            }

            // consume ch
            if (remaining.Length > 0 && remaining[0] == ch)
            {
                position++;
                remaining = source.Slice(position);
                return ReadResult.Good(position);
            }
            else
                return ReadResult.Fail();
        }

        public static bool IsLetterOrUnderscore(this char ch) => ch == '_' ? true : Char.IsLetter(ch);

        public static bool IsLetterOrDigitOrUnderscore(this char ch) => ch == '_' ? true : Char.IsLetterOrDigit(ch);

        public static ReadResult ConsumeExactString(ReadOnlySpan<char> source, string matchValue)
        {
            // todo case-insensitive match
            ReadOnlySpan<char> matchSpan = matchValue.AsSpan();
            if (matchSpan.Length == 0) return ReadResult.Fail();

            int position = 0;
            StringBuilder matched = new StringBuilder();

            while (source.Length > position && matchSpan.Length > position && source[position] == matchSpan[position])
            {
                matched.Append(source[position]);
                position++;
            }

            if (position == matchSpan.Length)
            {
                // matched all
                return ReadResult.Good(position, matched.ToString());
            }
            else
            {
                return ReadResult.Fail();
            }
        }

        public static ReadResult ConsumeAnyIdentifier(ReadOnlySpan<char> source)
        {
            int position = 0;
            StringBuilder matched = new StringBuilder();

            // consume any leading whitespace
            while (source.Length > position && char.IsWhiteSpace(source[position]))
            {
                position++;
            }

            // 1st char must be letter or '_'
            if (source.Length > position && source[position].IsLetterOrUnderscore())
            {
                matched.Append(source[position]);
                position++;
            }
            else
            {
                return ReadResult.Fail();
            }

            // other chars must be letter, digit or '_'
            while (source.Length > position && source[position].IsLetterOrDigitOrUnderscore())
            {
                matched.Append(source[position]);
                position++;
            }

            // return all matched
            return ReadResult.Good(position, matched.ToString());
        }

        public static ReadResult LoadField(this ReadOnlySpan<char> source, Dictionary<string, IField> fieldMap)
        {
            int position = 0;
            ReadOnlySpan<char> remaining = source.Slice(position);

            // consume Name
            var parsed = ConsumeAnyIdentifier(remaining);
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);
            if (parsed.Value is not string fieldName) return ReadResult.Fail();
            if (!fieldMap.TryGetValue(fieldName, out IField? field))
            {
                // unknown field - todo? ignore (old?) value
                throw new InvalidDataException($"Unknown field name '{fieldName}' at position {position}.");
            }

            // consume '='
            parsed = ConsumeChar(remaining, '=');
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            // consume value
            parsed = ConsumeAnyValue(remaining);
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            field.UntypedValue = parsed.Value;

            return ReadResult.Good(position);
        }

        public static ReadResult LoadFields(this ReadOnlySpan<char> source, params IField[] fields)
        {
            int position = 0;
            ReadOnlySpan<char> remaining = source.Slice(position);

            var fieldMap = new Dictionary<string, IField>();
            foreach (var field in fields)
            {
                fieldMap[field.Name] = field;
            }

            // consume first '{'
            var parsed = ConsumeChar(remaining, '{');
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            // consume fields
            int fieldsConsumed = 0;
            while (true)
            {
                if (remaining.Length == 0) return ReadResult.Fail();

                // try consume final '}'
                parsed = ConsumeChar(remaining, '}');
                if (parsed.Success)
                {
                    position += parsed.Consumed;
                    remaining = source.Slice(position);
                    return ReadResult.Good(position);
                }
                else
                {
                    // ignore
                }

                // consume field
                if (fieldsConsumed > 0)
                {
                    // consume ','
                    parsed = ConsumeChar(remaining, ',');
                    if (parsed.Failed) return ReadResult.Fail();
                    position += parsed.Consumed;
                    remaining = source.Slice(position);
                }
                parsed = LoadField(remaining, fieldMap);
                if (parsed.Failed) return ReadResult.Fail();
                position += parsed.Consumed;
                remaining = source.Slice(position);
                fieldsConsumed++;
            }
        }

        public static string ToText(this ITextable source)
        {
            StringBuilder builder = new StringBuilder();
            source.Emit(builder);
            return builder.ToString();
        }
    }
    internal interface IField
    {
        string Name { get; }
        Type Type { get; }
        object? UntypedValue { get; set; }
        bool IsDefault { get; }
    }
    internal sealed class ValType<T> : IField, IEquatable<ValType<T>> where T : struct, IEquatable<T>
    {
        private readonly string _name;
        private readonly Func<T?, bool> _isDefault;
        private T? _value;

        public string Name => _name;
        public Type Type => typeof(T);
        public bool IsDefault => _isDefault(_value);
        public T? Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public object? UntypedValue
        {
            get => _value;
            set
            {
                if (value is T tValue)
                {
                    _value = tValue;
                }
                else
                {
                    throw new InvalidOperationException($"Value ({value}) type is not {typeof(T)}");
                }
            }
        }

        public ValType(string name, T? value, Func<T?, bool> isDefault)
        {
            _name = name;
            _isDefault = isDefault;
            _value = value;
        }

        private static bool ValuesAreEqual(T? left, T? right)
        {
            if (left is null) return (right is null);
            return (right is null) ? false : left.Equals(right);
        }

        public bool Equals(ValType<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!ValuesAreEqual(other._value, _value)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is ValType<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_name, _value);
        public override string ToString() => $"{_name}<{typeof(T).Name}>={_value}";
    }
    internal sealed class RefType<T> : IField, IEquatable<RefType<T>> where T : class, IEquatable<T>
    {
        private readonly string _name;
        private readonly Func<T?, bool> _isDefault;
        private T? _value;

        public string Name => _name;
        public Type Type => typeof(T);
        public bool IsDefault => _isDefault(_value);
        public T? Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public object? UntypedValue
        {
            get => _value;
            set
            {
                if (value is T tValue)
                {
                    _value = tValue;
                }
                else
                {
                    throw new InvalidOperationException($"Value ({value}) type is not {typeof(T)}");
                }
            }
        }

        public RefType(string name, T? value, Func<T?, bool> isDefault)
        {
            _name = name;
            _isDefault = isDefault;
            _value = value;
        }

        private static bool ValuesAreEqual(T? left, T? right)
        {
            if (left is null) return (right is null);
            return (right is null) ? false : left.Equals(right);
        }

        public bool Equals(RefType<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!ValuesAreEqual(other._value, _value)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is RefType<T> other && Equals(other);
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

    internal sealed class Member : ITextable, IEquatable<Member>
    {
        private readonly ValType<int> _id = new ValType<int>(nameof(Id), 0, (i) => i == 0);
        public int Id { get => _id.Value ?? 0; set => _id.Value = value; }

        private readonly RefType<string> _name = new RefType<string>(nameof(Name), string.Empty, (s) => s == string.Empty);
        public string Name { get => _name.Value ?? ""; set => _name.Value = value; }

        private readonly RefType<string> _type = new RefType<string>(nameof(Type), string.Empty, (s) => s == string.Empty);
        public string Type { get => _type.Value ?? ""; set => _type.Value = value; }

        private readonly ValType<bool> _nullable = new ValType<bool>(nameof(Nullable), null, (i) => i is null);
        public bool? Nullable { get => _nullable.Value; set => _nullable.Value = value; }

        private readonly RefType<string> _desc = new RefType<string>(nameof(Description), null, (i) => i is null);
        public string? Description { get => _desc.Value; set => _desc.Value = value; }

        private readonly ValType<ValueArray<int>> _dims = new ValType<ValueArray<int>>(nameof(Dims), ValueArray<int>.Empty, (va) => (va?.Length ?? 0) == 0);
        public int[] Dims
        {
            get => (_dims.Value ?? ValueArray<int>.Empty).Values;
            set => _dims.Value = new ValueArray<int>(value);
        }
        public int Rank => _dims.Value?.Length ?? 0;

        public void Emit(StringBuilder builder) => builder.EmitFields(_id, _name, _type, _nullable, _desc, _dims);
        public bool Load(string source) => source.AsSpan().LoadFields(_id, _name, _type, _nullable, _desc, _dims).Success;

        public bool Equals(Member? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._id.Equals(_id)) return false;
            if (!other._name.Equals(_name)) return false;
            if (!other._type.Equals(_type)) return false;
            if (!other._nullable.Equals(_nullable)) return false;
            if (!other._desc.Equals(_desc)) return false;
            if (!other._dims.Equals(_dims)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Member other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_id, _name, _type, _nullable, _desc, _dims);
    }
    public class TextableTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("10%", "10%pc;")]
        [InlineData("{abc}", "%lc;abc%rc;")]
        [InlineData("(abc)", "%lp;abc%rp;")]
        [InlineData("[abc]", "%ls;abc%rs;")]
        [InlineData("<abc>", "%la;abc%ra;")]
        [InlineData("abc\\def", "abc%bs;def")]
        public void StringEscaping(string value, string expectedEncoding)
        {
            // encode
            string encoded = value.Escaped();
            encoded.ShouldBe(expectedEncoding);

            // decode
            string decoded = encoded.UnEscape();
            decoded.ShouldBe(value);
        }

        [Fact]
        public void Emit1_SimpleObject()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(False)}");
        }

        [Fact]
        public void Emit2_OptRefTypeA()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = null,
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(False)}");
        }

        [Fact]
        public void Emit3_OptRefTypeB()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = "abc",
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(False),Description=str(abc)}");
        }

        [Fact]
        public void Emit4_OptValTypeA()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String)}");
        }

        [Fact]
        public void Emit5_OptValTypeB()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = true,
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(True)}");
        }

        [Fact]
        public void Emit6_VectorValType()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Dims = new int[] { 2, 2, 2 },
            };
            string encoded1 = orig.ToText();
            encoded1.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Dims=i32[2,2,2]}");
        }

        [Theory]
        [InlineData(1, "{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(False)}")] // original encoding
        [InlineData(2, "{Id=i32(123),Type=str(System.String),Name=str(Field1),Nullable=log(False)}")] // field order re-arranged
        public void Load1_RearrangedEncoding(int _, string encoded)
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
            };

            Member copy = new Member();
            bool loaded = copy.Load(encoded);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Theory]
        [InlineData(1, " {Id=i32(123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(2, "{ Id=i32(123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(3, "{Id =i32(123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(4, "{Id= i32(123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(5, "{Id=i32 (123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(6, "{Id=i32( 123),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(7, "{Id=i32(123 ),Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(8, "{Id=i32(123) ,Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(9, "{Id=i32(123), Name=str(Field1),Type=str(System.String)}")] // random whitespace
        [InlineData(10, "{Id=i32(123),Name=str(Field1),Type=str(System.String) }")] // random whitespace
        [InlineData(11, "{Id=i32(123),Name=str(Field1),Type=str(System.String)} ")] // random whitespace
        public void Load2_RandomExtraWhitespace(int _, string encoded)
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };

            Member copy = new Member();
            bool loaded = copy.Load(encoded);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Load3_UnknownField()
        {
            string encoded = "{Id=i32(123),Name=str(Field1),Type=str(System.String),FieldX=str(abcdef)}";
            var ex = Assert.Throws<InvalidDataException>(() =>
            {
                Member copy = new Member();
                bool loaded = copy.Load(encoded);
                loaded.ShouldBeTrue();
            });
            ex.Message.ShouldBe("Unknown field name 'FieldX' at position 6.");
        }

        [Fact]
        public void Load4_IndentedInput()
        {
            const string encoded =
                """
                {
                    Id   = i32 (123)
                    ,
                    Name = str (Field1)
                    ,
                    Type = str (System.String)
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
            bool loaded = copy.Load(encoded);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Load5_Scalar()
        {
            Member orig = new Member()
            {
                Id = 123,
            };

            string encoded = orig.ToText();
            encoded.ShouldBe("{Id=i32(123)}");

            Member copy = new Member();
            bool loaded = copy.Load(encoded);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Load6_Vector()
        {
            Member orig = new Member()
            {
                Id = 123,
                Name = "Field1",
                Dims = new int[] { 2, 2, 2 },
            };

            string encoded = orig.ToText();
            encoded.ShouldBe("{Id=i32(123),Name=str(Field1),Dims=i32[2,2,2]}");

            Member copy = new Member();
            bool loaded = copy.Load(encoded);
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

    }
}