using DTOMaker.Gentime;
using System;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackModelScopeMember : ModelScopeMember
    {
        public MessagePackModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language, member)
        {
            MessagePackEntity entity = member.Entity as MessagePackEntity
                ?? throw new ArgumentException("Expected member.Entity to be a MessagePackEntity", nameof(member));
            var memberKeyOffset = entity.MemberKeyOffset;
            if (memberKeyOffset == 0)
            {
                int classHeight = member.Entity.GetClassHeight();
                memberKeyOffset = (classHeight - 1) * 100;
            }
            int memberKey = memberKeyOffset + member.Sequence;
            switch (member.Kind)
            {
                case MemberKind.Scalar:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "ScalarMemberKey"] = memberKey;
                    break;
                case MemberKind.Vector:
                    _tokens["VectorMemberKey"] = memberKey;
                    break;
                case MemberKind.Entity:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "EntityMemberKey"] = memberKey;
                    break;
                case MemberKind.Binary:
                    _tokens[(member.MemberIsNullable ? "Nullable" : "Required") + "BinaryMemberKey"] = memberKey;
                    break;
                default:
                    throw new NotImplementedException($"Member.Kind: {member.Kind}");
            }
        }
    }
}