using DTOMaker.Gentime;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackModelScopeDomain : ModelScopeDomain
    {
        public MessagePackModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
}
