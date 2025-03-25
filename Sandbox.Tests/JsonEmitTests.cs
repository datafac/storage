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

    public readonly struct SourceToken
    {
        public readonly TokenKind Kind;
        public readonly SourceLine Source;
        public readonly int Offset;
        public readonly int Length;
        public readonly string Message;

        public SourceToken(TokenKind kind, SourceLine source, int offset, int length, string message = "")
        {
            Kind = kind;
            Source = source;
            Offset = offset;
            Length = length;
            Message = message;
        }

        public string StringValue => Source.Text.Substring(Offset, Length);

        public SourceToken Extend() => new SourceToken(Kind, Source, Offset, Length + 1);

        public override string ToString()
        {
            return Kind switch
            {
                TokenKind.Whitespace => " ",
                _ => Source.Text.Substring(Offset, Length),
            };
        }
    }

    public readonly struct LoadResult
    {
        public readonly bool Success;
        public readonly int Consumed;
        public readonly SourceToken Token;

        public bool IsError => Token.Kind == TokenKind.Error;

        /// <summary>
        /// Returns a successful result.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="consumed"></param>
        /// <param name="token"></param>
        public LoadResult(int consumed, SourceToken token)
        {
            Success = true;
            Consumed = consumed;
            Token = token;
        }

        /// <summary>
        /// Returns a failed result with an error message
        /// </summary>
        /// <param name="message"></param>
        public LoadResult(SourceToken token)
        {
            Success = false;
            Consumed = 0;
            Token = token;
        }
    }

    internal interface ITextable
    {
        LoadResult Load(TextReader reader);
        void Emit(TextWriter writer, int indent);
    }
    internal static class LexChar
    {
        public const char Percent = '%';
        public const char SemiColon = ';';
        public const char DblQuote = '"';
        public const char BackSlash = '\\';
        public const char EqualCh = '=';
        public const char Comma = ',';
        public const char LeftParen = '(';
        public const char RightParen = ')';
        public const char LeftCurly = '{';
        public const char RightCurly = '}';
        public const char LeftSquare = '[';
        public const char RightSquare = ']';
        public const char LeftAngle = '<';
        public const char RightAngle = '>';
    }
    internal static class EscCode
    {
        public const string Percent = "pc";
        public const string SemiColon = "sc";
        public const string DblQuote = "dq";
        public const string BackSlash = "bs";
        public const string EqualCh = "eq";
        public const string Comma = "cm";
        public const string LeftParen = "lp";
        public const string RightParen = "rp";
        public const string LeftCurly = "lc";
        public const string RightCurly = "rc";
        public const string LeftSquare = "ls";
        public const string RightSquare = "rs";
        public const string LeftAngle = "la";
        public const string RightAngle = "ra";
    }
    internal static class TextableExtensions
    {
        private static readonly Dictionary<char, string> _map = new Dictionary<char, string>()
        {
            // char             code
            [LexChar.Percent] = EscCode.Percent,
            [LexChar.SemiColon] = EscCode.SemiColon,
            [LexChar.DblQuote] = EscCode.DblQuote,
            [LexChar.BackSlash] = EscCode.BackSlash,
            [LexChar.EqualCh] = EscCode.EqualCh,   
            [LexChar.Comma] = EscCode.Comma,
            [LexChar.LeftParen] = EscCode.LeftParen,
            [LexChar.RightParen] = EscCode.RightParen,
            [LexChar.LeftCurly] = EscCode.LeftCurly,
            [LexChar.RightCurly] = EscCode.RightCurly,
            [LexChar.LeftSquare] = EscCode.LeftSquare,
            [LexChar.RightSquare] = EscCode.RightSquare,
            [LexChar.LeftAngle] = EscCode.LeftAngle,
            [LexChar.RightAngle] = EscCode.RightAngle,
        };
        private static ImmutableDictionary<char, string> BuildCharToCodeMap()
        {
            return ImmutableDictionary<char, string>.Empty.AddRange(_map
                .Where(kvp => kvp.Value.Length == 2));
        }
        private static ImmutableDictionary<string, char> BuildCodeToCharMap()
        {
            return ImmutableDictionary<string, char>.Empty.AddRange(_map
                .Where(kvp => kvp.Value.Length == 2)
                .Select(kvp => new KeyValuePair<string, char>(kvp.Value, kvp.Key)));
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
            if (span.IndexOf(LexChar.Percent) < 0) return value;

            StringBuilder result = new StringBuilder();
            int pos = 0;
            while (pos < span.Length)
            {
                char ch = span[pos];
                if (ch == LexChar.Percent)
                {
                    if (pos + 3 < span.Length && span[pos + 3] == LexChar.SemiColon)
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
                            // invalid escape code - just emit as is
                            result.Append(LexChar.Percent);
                            result.Append(code);
                            result.Append(LexChar.SemiColon);
                        }
                        // next
                        pos = pos + 4;
                    }
                    else
                    {
                        // bad format - just emit as is
                        result.Append(ch);
                        //next
                        pos++;
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
                        else if (ch == LexChar.DblQuote)
                        {
                            // start of string
                            token = new SourceToken(TokenKind.String, sourceLine, offset + 1, 0);
                        }
                        else
                        {
                            yield return ch switch
                            {
                                LexChar.LeftCurly => new SourceToken(TokenKind.LeftCurly, sourceLine, offset, 1),
                                LexChar.RightCurly => new SourceToken(TokenKind.RightCurly, sourceLine, offset, 1),
                                LexChar.Comma => new SourceToken(TokenKind.Comma, sourceLine, offset, 1),
                                LexChar.LeftSquare => new SourceToken(TokenKind.LeftSquare, sourceLine, offset, 1),
                                LexChar.RightSquare => new SourceToken(TokenKind.RightSquare, sourceLine, offset, 1),
                                LexChar.EqualCh => new SourceToken(TokenKind.Equals, sourceLine, offset, 1),
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
                        if (ch == LexChar.DblQuote)
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
            writer.Write(LexChar.LeftCurly);
            indent += 4;
            int emitted = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsDefaultValue)
                {
                    if (emitted > 0) writer.Write(LexChar.Comma);
                    writer.EmitField(indent, field);
                    emitted++;
                }
            }
            indent -= 4;
            writer.WriteLine();
            writer.Write(new string(' ', indent));
            writer.Write(LexChar.RightCurly);
        }

        private static LoadResult TryLoadOneToken(ReadOnlySpan<SourceToken> tokens, TokenKind tokenKind)
        {
            if (tokens.Length <= 0)
                return new LoadResult(default);
            var token = tokens[0];
            if (token.Kind == TokenKind.Error)
                return new LoadResult(token);
            if (token.Kind == tokenKind)
                return new LoadResult(1, token);
            else
                return new LoadResult(default);
        }

        private static LoadResult MustLoadOneToken(ReadOnlySpan<SourceToken> tokens, TokenKind tokenKind)
        {
            if (tokens.Length <= 0)
                return new LoadResult(new SourceToken(TokenKind.Error, default, 0, 0,
                    $"Unexpected EOF. Expected '{tokenKind}'."));

            var token = tokens[0];
            if (token.Kind == TokenKind.Error) return new LoadResult(token);
            if (token.Kind == tokenKind)
                return new LoadResult(1, token);
            else
                return new LoadResult(new SourceToken(TokenKind.Error, token.Source, token.Offset, token.Length,
                    $"Unexpected token. Expected '{tokenKind}', received '{token.Kind}' at (L{token.Source.Line},C{token.Offset})."));
        }

        public static LoadResult TryLoadScalar(ReadOnlySpan<SourceToken> remaining, Func<string, bool> valueHandler)
        {
            // try consume string value
            var result = TryLoadOneToken(remaining, TokenKind.String);
            if (result.IsError) return result;
            if (result.Success && valueHandler(result.Token.StringValue)) return new LoadResult(1, result.Token);

            // try consume number value
            result = TryLoadOneToken(remaining, TokenKind.Number);
            if (result.IsError) return result;
            if (result.Success && valueHandler(result.Token.StringValue)) return new LoadResult(1, result.Token);

            // try consume identifier value e.g. true or false
            result = TryLoadOneToken(remaining, TokenKind.Identifier);
            if (result.IsError) return result;
            if (result.Success && valueHandler(result.Token.StringValue)) return new LoadResult(1, result.Token);

            return new LoadResult(default);
        }

        public static LoadResult TryLoadVector(ReadOnlySpan<SourceToken> remaining, Func<string, bool> valueHandler)
        {
            int tokensConsumed = 0;

            // try consume begin vector
            var result = TryLoadOneToken(remaining, TokenKind.LeftSquare);
            if (result.IsError) return result;
            if (!result.Success) return result;
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            int fieldsConsumed = 0;
            while (true)
            {
                if (remaining.Length == 0) return new LoadResult(default);

                // try consume close vector
                result = TryLoadOneToken(remaining, TokenKind.RightSquare);
                if (result.IsError) return result;
                if (result.Success)
                {
                    remaining = remaining.Slice(1);
                    tokensConsumed += 1;
                    return new LoadResult(tokensConsumed, default);
                }

                // try consume value separator
                if (fieldsConsumed > 0)
                {
                    result = TryLoadOneToken(remaining, TokenKind.Comma);
                    if (result.IsError) return result;
                    if (!result.Success) return new LoadResult(default);
                    remaining = remaining.Slice(1);
                    tokensConsumed += 1;
                }

                // try consume scalar
                result = TryLoadScalar(remaining, valueHandler);
                if (result.IsError) return result;
                if (!result.Success) return result;
                remaining = remaining.Slice(result.Consumed);
                tokensConsumed += result.Consumed;
                fieldsConsumed++;
            }
        }

        private static LoadResult TryLoadField(ReadOnlySpan<SourceToken> remaining, Dictionary<string, IField> fieldMap)
        {
            int tokensConsumed = 0;

            // try consume field
            var result = TryLoadOneToken(remaining, TokenKind.Identifier);
            if (result.IsError) return result;
            if (!result.Success) return new LoadResult(default);
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            // check field name
            Func<string, bool> valueParser = (string s) => { return true; };
            string fieldName = result.Token.StringValue;
            if (fieldMap.TryGetValue(fieldName, out IField? field))
            {
                valueParser = field.ValueParser;
            }
            else
            {
                // ignore unknown (old? or new/) field name
            }

            // try consume equals
            result = TryLoadOneToken(remaining, TokenKind.Equals);
            if(result.IsError) return result;
            if (!result.Success) return new LoadResult(default);
            remaining = remaining.Slice(1);
            tokensConsumed += 1;

            // try consume value as vector
            result = TryLoadVector(remaining, valueParser);
            if (result.IsError) return result;
            if (result.Success)
            {
                remaining = remaining.Slice(result.Consumed);
                tokensConsumed += result.Consumed;
                return new LoadResult(tokensConsumed, default);
            }

            // try consume value as scalar
            result = TryLoadScalar(remaining, valueParser);
            if (result.IsError) return result;
            if (result.Success)
            {
                remaining = remaining.Slice(result.Consumed);
                tokensConsumed += result.Consumed;
                return new LoadResult(tokensConsumed, default);
            }

            return new LoadResult(default);
        }

        private static LoadResult TryLoadFields(ReadOnlySpan<SourceToken> remaining, Dictionary<string, IField> fieldMap)
        {
            int position = 0;

            // consume begin field group
            var result = TryLoadOneToken(remaining, TokenKind.LeftCurly);
            if(result.IsError) return result;
            if (!result.Success) return new LoadResult(default);
            remaining = remaining.Slice(1);
            position += 1;

            // consume fields
            int fieldsConsumed = 0;
            while (true)
            {
                if (remaining.Length == 0) return new LoadResult(default);

                // try consume close field group
                result = TryLoadOneToken(remaining, TokenKind.RightCurly);
                if (result.IsError) return result;
                if (result.Success)
                {
                    remaining = remaining.Slice(1);
                    position += 1;
                    return new LoadResult(position, default);
                }

                // try consume field separator
                if (fieldsConsumed > 0)
                {
                    result = MustLoadOneToken(remaining, TokenKind.Comma);
                    if (result.IsError) return result;
                    if (!result.Success) return new LoadResult(default);
                    remaining = remaining.Slice(1);
                    position += 1;
                }

                // try consume field
                result = TryLoadField(remaining, fieldMap);
                if (result.IsError) return result;
                if (result.Success)
                {
                    remaining = remaining.Slice(result.Consumed);
                    position += result.Consumed;
                    fieldsConsumed++;
                }
            }
        }

        public static LoadResult LoadFields(this TextReader reader, params IField[] fields)
        {
            var fieldMap = new Dictionary<string, IField>();
            foreach (var field in fields)
            {
                fieldMap[field.Name] = field;
            }

            // ignore whitespace
            ReadOnlySpan<SourceToken> tokens = reader.ReadAllTokens().Where(t => t.Kind != TokenKind.Whitespace).ToArray().AsSpan();

            // consume token stream
            return TryLoadFields(tokens, fieldMap);
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
        private static readonly FieldMetaInt _instance = new FieldMetaInt();
        public static FieldMetaInt Instance => _instance;

        public int DefaultValue => default;
        public string FormatValue(int value) => value.ToString();
        public bool IsDefaultValue(int value) => value == default;
        public bool TryParseValue(string input, out int result) => int.TryParse(input, out result);
    }
    internal sealed class FieldMetaBool : IFieldMeta<bool>
    {
        private static readonly FieldMetaBool _instance = new FieldMetaBool();
        public static FieldMetaBool Instance => _instance;

        public bool DefaultValue => default;
        public string FormatValue(bool value) => value.ToString().ToLower();
        public bool IsDefaultValue(bool value) => value == default;
        public bool TryParseValue(string input, out bool result) => bool.TryParse(input, out result);
    }
    internal sealed class FieldMetaByte : IFieldMeta<byte>
    {
        private static readonly FieldMetaByte _instance = new FieldMetaByte();
        public static FieldMetaByte Instance => _instance;

        public byte DefaultValue => default;
        public string FormatValue(byte value) => value.ToString().ToLower();
        public bool IsDefaultValue(byte value) => value == default;
        public bool TryParseValue(string input, out byte result) => byte.TryParse(input, out result);
    }
    internal sealed class FieldMetaString : IFieldMeta<string>
    {
        private static readonly FieldMetaString _instance = new FieldMetaString();
        public static FieldMetaString Instance => _instance;

        public string DefaultValue => string.Empty;
        public string FormatValue(string value)
        {
            StringBuilder result = new StringBuilder();
            result.Append(LexChar.DblQuote);
            result.Append(value.Escaped());
            result.Append(LexChar.DblQuote);
            return result.ToString();
        }
        public bool IsDefaultValue(string value) => value == string.Empty;
        public bool TryParseValue(string input, out string result)
        {
            result = input.UnEscape();
            return true;
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

        public string Name => _name;
        public Type Type => typeof(T);
    }
    internal sealed class ScalarValType<T> : AnyType<T>, IField, IEquatable<ScalarValType<T>> where T : struct, IEquatable<T>
    {
        private T? _value;

        public ScalarValType(IFieldMeta<T> meta, string name, bool nullable = false) : base(meta, name, nullable)
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
            string formatted = _value is null ? "null" : _meta.FormatValue(_value.Value);
            writer.Write(formatted);
        }
    }

    internal sealed class ScalarRefType<T> : AnyType<T>, IField, IEquatable<ScalarRefType<T>> where T : class, IEquatable<T>
    {
        private T? _value;

        public ScalarRefType(IFieldMeta<T> meta, string name, bool nullable = false) : base(meta, name, nullable)
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
            string formatted = _value is null ? "null" : _meta.FormatValue(_value);
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

    internal sealed class VectorRefType<T> : AnyType<T>, IField, IEquatable<VectorRefType<T>> where T : class, IEquatable<T>, new()
    {
        private T?[] _values;

        public VectorRefType(IFieldMeta<T> meta, string name, bool nullable = false) : base(meta, name, nullable)
        {
            _values = Array.Empty<T?>();
        }

        public bool IsDefaultValue => _values.Length == 0;
        public T?[] NullableValues
        {
            get { return _values; }
            set { _values = value; }
        }
        public T[] Values
        {
            get { return _values.Select(x => x ?? _meta.DefaultValue).ToArray(); }
            set { _values = value; }
        }

        public void WriteValue(TextWriter writer, int indent)
        {
            writer.Write(LexChar.LeftSquare);
            int count = 0;
            foreach (var value in _values)
            {
                if (count > 0)
                    writer.Write(LexChar.Comma);
                // todo indent
                string formatted = value is null ? "null" : _meta.FormatValue(value);
                writer.Write(formatted);
            }
            writer.Write(LexChar.RightSquare);
        }

        public bool ValueParser(string value)
        {
            //return _meta.TryParseValue(value, out _value);
            throw new NotImplementedException();
        }

        private static bool ValuesAreEqual(T? left, T? right)
        {
            if (left is null) return (right is null);
            return (right is null) ? false : left.Equals(right);
        }

        private static bool ArraysAreEqual(T?[] left, T?[] right)
        {
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (!ValuesAreEqual(left[i], right[i])) return false;
            }
            return true;
        }

        public bool Equals(VectorRefType<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            if (!ArraysAreEqual(other._values, _values)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is VectorRefType<T> other && Equals(other);
        public override int GetHashCode()
        {
            HashCode result = new HashCode();
            result.Add(_name);
            result.Add(_values.Length);
            for (int i = 0; i < _values.Length; i++)
            {
                result.Add(_values[i]);
            }
            return result.ToHashCode();
        }

        public override string ToString()
        {
            //return $"{_name}<{typeof(T).Name}>={_value}";
            throw new NotImplementedException();
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

    internal sealed class Simple : ITextable, IEquatable<Simple>
    {
        private readonly ScalarValType<int> _id = new ScalarValType<int>(FieldMetaInt.Instance, nameof(Id), false);
        public int Id { get => _id.Value; set => _id.Value = value; }

        private readonly ScalarRefType<string> _name = new ScalarRefType<string>(FieldMetaString.Instance, nameof(Name));
        public string Name { get => _name.Value; set => _name.Value = value; }

        private readonly ScalarRefType<string> _type = new ScalarRefType<string>(FieldMetaString.Instance, nameof(Type));
        public string Type { get => _type.Value; set => _type.Value = value; }

        private readonly ScalarValType<bool> _nullable = new ScalarValType<bool>(FieldMetaBool.Instance, nameof(Nullable), true);
        public bool? Nullable { get => _nullable.NullableValue; set => _nullable.NullableValue = value; }

        private readonly ScalarRefType<string> _desc = new ScalarRefType<string>(FieldMetaString.Instance, nameof(Description), true);
        public string? Description { get => _desc.NullableValue; set => _desc.NullableValue = value; }

        private readonly ScalarValType<byte> _kind1 = new ScalarValType<byte>(FieldMetaByte.Instance, nameof(Kind1), false);
        public MyEnum1 Kind1 { get => (MyEnum1)_kind1.Value; set => _kind1.Value = (byte)value; }

        private readonly ScalarValType<int> _kind2 = new ScalarValType<int>(FieldMetaInt.Instance, nameof(Kind2), true);
        public MyEnum2? Kind2 { get => (MyEnum2?)_kind2.NullableValue; set => _kind2.NullableValue = (int?)value; }

        public void Emit(TextWriter writer, int indent) => writer.EmitFields(indent, _id, _name, _type, _nullable, _desc, _kind1, _kind2);
        public LoadResult Load(TextReader reader) => reader.LoadFields(_id, _name, _type, _nullable, _desc, _kind1, _kind2);

        public bool Equals(Simple? other)
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
        public override bool Equals(object? obj) => obj is Simple other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_id, _name, _type, _nullable, _desc, _kind1, _kind2);

    }

    internal sealed class Member : ITextable, IEquatable<Member>, IFieldMeta<Member>
    {
        private static readonly IFieldMeta<Member> _meta = new Member();
        public static IFieldMeta<Member> Meta => _meta;

        public Member DefaultValue => new Member();
        public bool IsDefaultValue(Member value)
        {
            throw new NotImplementedException();
        }

        public string FormatValue(Member value)
        {
            throw new NotImplementedException();
        }

        public bool TryParseValue(string input, out Member result)
        {
            throw new NotImplementedException();
        }

        private readonly ScalarRefType<string> _name = new ScalarRefType<string>(FieldMetaString.Instance, nameof(Name));
        public string Name { get => _name.Value; set => _name.Value = value; }

        public void Emit(TextWriter writer, int indent) => writer.EmitFields(indent, _name);
        public LoadResult Load(TextReader reader) => reader.LoadFields(_name);

        public bool Equals(Member? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._name.Equals(_name)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Member other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_name);
    }

    internal sealed class Family : ITextable, IEquatable<Family>
    {
        private readonly ScalarRefType<string> _surname = new ScalarRefType<string>(FieldMetaString.Instance, nameof(Surname));
        public string Surname { get => _surname.Value; set => _surname.Value = value; }

        private readonly ScalarRefType<Member> _leader = new ScalarRefType<Member>(Member.Meta, nameof(Leader), true);
        public Member? Leader { get => _leader.NullableValue; set => _leader.NullableValue = value; }

        //private readonly VectorRefType<Member> _members = new VectorRefType<Member>(nameof(Members));
        //public Member[] Members { get => _members.Values; set => _members.Values = value; }

        public void Emit(TextWriter writer, int indent) => writer.EmitFields(indent, _surname, _leader);
        public LoadResult Load(TextReader reader) => reader.LoadFields(_surname, _leader);

        public bool Equals(Family? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!other._surname.Equals(_surname)) return false;
            if (!other._leader.Equals(_leader)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Family other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_surname, _leader);
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
        public async Task Basic0_EmptyObject()
        {
            Simple orig = new Simple();

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Basic1_SimpleObject()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
            };

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]

        public async Task Basic2_OptRefTypeA()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = null,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Basic3_OptRefTypeB()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = false,
                Description = "abc",
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Basic4_OptValTypeA()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Basic5_OptValTypeB()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = true,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public async Task Basic6_ReqEnumType()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(MyEnum1).FullName!,
                //Nullable = true,
                Kind1 = MyEnum1.First,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);

            copy.Kind1.ShouldBe(MyEnum1.First);
        }

        [Fact]
        public async Task Basic7_OptEnumType()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(MyEnum2).FullName!,
                //Nullable = true,
                //Kind2 = MyEnum2.First,
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);

            copy.Kind2.HasValue.ShouldBeFalse();
            copy.Kind2.ShouldBeNull();
        }

        [Fact]
        public async Task Basic8_VectorValType()
        {
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                //Dims = new int[] { 2, 2, 2 },
            };
            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Simple copy = new Simple();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
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
            token.Message.ShouldBe("Unexpected character");
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
            token.Message.ShouldBe("Unterminated string");
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
            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            bool loaded = copy.Load(reader).Success;
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

            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
                Nullable = null,
            };

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Parser3_Success_UnknownFieldsAreIgnored()
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

            Simple orig = new Simple()
            {
                Id = 123,
                Name = "Field1",
                Type = typeof(string).FullName!,
            };

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

        [Fact]
        public void Parser4_Failure_UnterminatedStringValue()
        {
            string encoded =
                """
                {
                    Id=123,
                    Name="Field1",
                    Type="System.String
                }
                """;

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            LoadResult result = copy.Load(reader);
            result.Success.ShouldBeFalse();
            result.Token.Kind.ShouldBe(TokenKind.Error);
            result.Token.Message.ShouldBe("Unterminated string");
        }

        [Fact]
        public void Parser5_Failure_MissingSeparatingComma()
        {
            string encoded =
                """
                {
                    Id=123
                    Name="Field1"
                }
                """;

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            LoadResult result = copy.Load(reader);
            result.Success.ShouldBeFalse();
            result.Token.Kind.ShouldBe(TokenKind.Error);
            result.Token.Message.ShouldBe("Unexpected token. Expected 'Comma', received 'Identifier' at (L2,C4).");
        }

        [Fact]
        public void Parser6_Failure_ExtraSeparatingCommas()
        {
            string encoded =
                """
                {
                    Id=123,,
                    Name="Field1",
                }
                """;

            Simple copy = new Simple();
            using var reader = new StringReader(encoded);
            LoadResult result = copy.Load(reader);
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Nested0_Roundtrip_Empty()
        {
            Family orig = new Family();

            string emitted = orig.ToText();
            await Verifier.Verify(emitted);

            Family copy = new Family();
            using var reader = new StringReader(emitted);
            bool loaded = copy.Load(reader).Success;
            loaded.ShouldBeTrue();

            copy.ShouldBe(orig);
        }

    }
}