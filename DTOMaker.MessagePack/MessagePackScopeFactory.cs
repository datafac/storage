using DTOMaker.Gentime;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new MessagePackModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new MessagePackModelScopeMember(parent, factory, language, member);
        }
    }
}
