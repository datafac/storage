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
            _variables["MemberKey"] = memberKey;
            _variables["ScalarMemberKey"] = memberKey;
            if (member.MemberIsNullable)
                _variables["ScalarNullableMemberKey"] = memberKey;
            else
                _variables["ScalarRequiredMemberKey"] = memberKey;
            _variables["VectorMemberKey"] = memberKey;
        }
    }
}
