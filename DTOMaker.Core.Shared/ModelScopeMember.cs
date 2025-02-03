using System;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeMember : ModelScopeBase
    {
        private static string ToCamelCase(string value)
        {
            ReadOnlySpan<char> input = value.AsSpan();
            Span<char> output = stackalloc char[input.Length];
            input.CopyTo(output);
            for (int i = 0; i < output.Length; i++)
            {
                if (Char.IsLetter(output[i]))
                {
                    output[i] = Char.ToLower(output[i]);
                    return new string(output.ToArray());
                }
            }
            return new string(output.ToArray());
        }

        private readonly TargetMember _member;
        public ModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language)
        {
            _member = member;
            _tokens["MemberIsObsolete"] = member.IsObsolete;
            _tokens["MemberObsoleteMessage"] = member.ObsoleteMessage;
            _tokens["MemberObsoleteIsError"] = member.ObsoleteIsError;
            _tokens["MemberType"] = _language.GetDataTypeToken(member.MemberType);
            _tokens["MemberTypeName"] = member.MemberType.ShortName;
            _tokens["MemberTypeNameSpace"] = member.MemberType.NameSpace;
            _tokens["MemberIsNullable"] = member.MemberIsNullable;
            _tokens["MemberIsValueType"] = member.MemberIsValueType;
            _tokens["MemberIsReferenceType"] = member.MemberIsReferenceType;
            _tokens["MemberSequence"] = member.Sequence;
            _tokens["MemberName"] = member.Name;
            _tokens["MemberJsonName"] = ToCamelCase(member.Name);
            _tokens["MemberDefaultValue"] = _language.GetDefaultValue(member.MemberType);
            switch (member.Kind)
            {
                case MemberKind.Scalar:
                    _tokens["ScalarMemberSequence"] = member.Sequence;
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "ScalarMemberSequence"] = member.Sequence;
                    _tokens["ScalarMemberName"] = member.Name;
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "ScalarMemberName"] = member.Name;
                    break;
                case MemberKind.Vector:
                    _tokens["VectorMemberSequence"] = member.Sequence;
                    _tokens["VectorMemberName"] = member.Name;
                    break;
                case MemberKind.Entity:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "EntityMemberName"] = member.Name;
                    break;
                //case MemberKind.Binary:
                //    break;
                default:
                    throw new NotImplementedException($"Member.Kind: {member.Kind}");
            }
        }

        public MemberKind Kind => _member.Kind;
        public bool IsScalar => _member.Kind == MemberKind.Scalar;
        public bool IsVector => _member.Kind == MemberKind.Vector;
        public bool IsEntity => _member.Kind == MemberKind.Entity;
        public bool IsNullable => _member.MemberIsNullable;
        public bool IsObsolete => _member.IsObsolete;
        public int FieldLength => _member.FieldLength;
    }
}
