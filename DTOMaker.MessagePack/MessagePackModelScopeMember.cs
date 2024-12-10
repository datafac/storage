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
            int memberTag = entity.MemberTagOffset + member.Sequence;
            _variables["MemberTag"] = memberTag;
            _variables["ScalarMemberTag"] = memberTag;
            if (member.MemberIsNullable)
                _variables["ScalarNullableMemberTag"] = memberTag;
            else
                _variables["ScalarRequiredMemberTag"] = memberTag;
            _variables["VectorMemberTag"] = memberTag;
        }
    }
}
