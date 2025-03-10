using DTOMaker.Gentime;

namespace DTOMaker.CSRecord
{
    public sealed class CSRecordScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new CSRecordModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new CSRecordModelScopeMember(parent, factory, language, member);
        }
    }
}
