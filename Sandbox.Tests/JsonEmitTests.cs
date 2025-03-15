using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
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
    internal static class TextIOExtensions
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

        public static void EmitField(this StringBuilder builder, IField field)
        {
            builder.Append(field.Name);
            builder.Append('=');
            object? value = field.UntypedValue;
            string formatted = value switch
            {
                null => "nul",
                bool b => $"log({b})",
                int i => $"i32({i})",
                long l => $"i64({l})",
                float f => $"r32({f.ToString("R")})",
                double d => $"r64({d.ToString("R")})",
                string s => $"str({s.Escaped()})",
                _ => $"unk({(value?.ToString() ?? "").Escaped()})"
            };
            builder.Append(formatted);
        }

        public static ReadResult ConsumeGroup(ReadOnlySpan<char> source, char beginGroup, char closeGroup)
        {
            int position = 0;
            var remaining = source.Slice(position);
            StringBuilder matched = new StringBuilder();

            // match begin char
            if (remaining.Length < 2 || remaining[0] != beginGroup) return ReadResult.Fail();
            position++;
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

        public static ReadResult ConsumeAnyValue(ReadOnlySpan<char> source)
        {
            int position = 0;
            var remaining = source.Slice(position);
            StringBuilder matched = new StringBuilder();

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
                "i32" => true,
                "i64" => true,
                "r32" => true,
                "r64" => true,
                "str" => true,
                _ => false,
            };

            if (!valid) return ReadResult.Fail();

            // consume any text grouped by '(' and ')'
            var parsed = ConsumeGroup(remaining, '(', ')');
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);

            // unescape value
            if (!parsed.HasValue) return ReadResult.Fail();
            string unparsed = (parsed.Value as string ?? "").UnEscape();
            object? value = prefix switch
            {
                "log" => bool.TryParse(unparsed, out bool bValue) ? bValue : null,
                "i32" => int.TryParse(unparsed, out int iValue) ? iValue : null,
                "i64" => long.TryParse(unparsed, out long lValue) ? lValue : null,
                "r32" => float.TryParse(unparsed, out float fValue) ? fValue : null,
                "r64" => double.TryParse(unparsed, out double dValue) ? dValue : null,
                "str" => unparsed,
                _ => false,
            };
            return value is null ? ReadResult.Fail() : ReadResult.Good(position, value);
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
            if (source.Length >= 1 && source[0] == ch)
                return ReadResult.Good(1);
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

            if (position == matched.Length)
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

        public static ReadResult LoadField(this ReadOnlySpan<char> source, IField field)
        {
            int position = 0;
            ReadOnlySpan<char> remaining = source.Slice(position);

            // consume Name
            var parsed = ConsumeExactString(remaining, field.Name);
            if (parsed.Failed) return ReadResult.Fail();
            position += parsed.Consumed;
            remaining = source.Slice(position);
            if (parsed.Value is not string fieldId) return ReadResult.Fail();

            // consume '='
            if (remaining.Length == 0 || remaining[0] != '=') return ReadResult.Fail();
            position++;
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

            // consume first '{'
            if (remaining.Length == 0 || remaining[0] != '{') return ReadResult.Fail();
            position++;
            remaining = source.Slice(position);

            // consume fields
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                {
                    // consume ','
                    if (remaining.Length == 0 || remaining[0] != ',') return ReadResult.Fail();
                    position++;
                    remaining = source.Slice(position);
                }

                // consume field
                var parsed1 = LoadField(remaining, fields[i]);
                if (parsed1.Failed) return ReadResult.Fail();
                position += parsed1.Consumed;
                remaining = source.Slice(position);
            }

            // consume final '}'
            if (remaining.Length == 0 || remaining[0] != '}') return ReadResult.Fail();
            position++;
            remaining = source.Slice(position);

            return ReadResult.Good(position);
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
    internal sealed class Field<T> : IField, IEquatable<Field<T>> where T: IEquatable<T>
    {
        private readonly string _name;
        private readonly Func<T,bool> _isDefault;
        private T _value;

        public string Name => _name;
        public Type Type => typeof(T);
        public bool IsDefault => _isDefault(_value);
        public T Value
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

        public Field(string name, T value, Func<T, bool> isDefault)
        {
            _name = name;
            _isDefault = isDefault;
            _value = value;
        }

        public bool Equals(Field<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!other._value.Equals(_value)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is Field<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_name, _value);
        public override string ToString() => $"{_name}<{typeof(T).Name}>={_value}";
    }

    internal sealed class Member : ITextable, IEquatable<Member>
    {
        private Field<int> _id = new Field<int>(nameof(Id), 0, (i) => i == 0);
        public int Id
        {
            get => _id.Value; 
            set => _id.Value = value;
        }

        private Field<string> _name = new Field<string>(nameof(Name), string.Empty, (s) => s == string.Empty);
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        private Field<string> _type = new Field<string>(nameof(Type), string.Empty, (s) => s == string.Empty);
        public string Type
        {
            get => _type.Value;
            set => _type.Value = value;
        }

        private Field<bool> _nullable = new Field<bool>(nameof(Nullable), false, (i) => i == false);
        public bool Nullable
        {
            get => _nullable.Value;
            set => _nullable.Value = value;
        }

        public void Emit(StringBuilder builder) => builder.EmitFields(_id, _name, _type, _nullable);
        public bool Load(string source) => source.AsSpan().LoadFields(_id, _name, _type, _nullable).Success;

        public bool Equals(Member? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._id.Equals(_id)) return false;
            if (!other._name.Equals(_name)) return false;
            if (!other._type.Equals(_type)) return false;
            if (!other._nullable.Equals(_nullable)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Member other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_id, _name, _type, _nullable);
    }
    public class TextIOTests
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
        public void Roundtrip1_SingleObject()
        {
            Member orig = new Member() { 
                Id = 123, 
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = true,
            };
            string encoded = orig.ToText();
            encoded.ShouldBe("{Id=i32(123),Name=str(Field1),Type=str(System.String),Nullable=log(True)}");

            Member copy = new Member();
            copy.Load(encoded);

            copy.ShouldBe(orig);
        }
    }
}