using DTOMaker.Gentime;
using System;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeMember : ModelScopeMember
    {
        public MemBlocksModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language, member)
        {
            MemBlockEntity entity = member.Entity as MemBlockEntity
                ?? throw new ArgumentException("Expected member.Entity to be a MemBlocksEntity", nameof(member));
        }
    }
}
