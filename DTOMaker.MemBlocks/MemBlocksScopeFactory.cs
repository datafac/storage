using DTOMaker.Gentime;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new MemBlocksModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new MemBlocksModelScopeMember(parent, factory, language, member);
        }
    }
}
