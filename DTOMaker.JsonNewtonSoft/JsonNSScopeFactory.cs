using DTOMaker.Gentime;

namespace DTOMaker.JsonNewtonSoft
{
    public sealed class JsonNSScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new JsonNSModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new JsonNSModelScopeMember(parent, factory, language, member);
        }
    }
}
