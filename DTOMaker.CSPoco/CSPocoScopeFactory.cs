using DTOMaker.Gentime;

namespace DTOMaker.CSPoco
{
    public sealed class CSPocoScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new CSPocoModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new CSPocoModelScopeMember(parent, factory, language, member);
        }
    }
}
