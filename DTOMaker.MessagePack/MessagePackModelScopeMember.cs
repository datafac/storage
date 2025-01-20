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
            _tokens["MemberKey"] = memberKey;
            _tokens["ScalarMemberKey"] = memberKey;
            if (member.MemberIsNullable)
                _tokens["NullableScalarMemberKey"] = memberKey;
            else
                _tokens["RequiredScalarMemberKey"] = memberKey;
            if (member.MemberIsVector)
                _tokens["VectorMemberKey"] = memberKey;
            if (member.MemberIsEntity)
            {
                if (member.MemberIsNullable)
                    _tokens["NullableEntityMemberKey"] = memberKey;
                else
                    _tokens["RequiredEntityMemberKey"] = memberKey;
            }
        }
    }
}