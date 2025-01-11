using DTOMaker.Gentime;

namespace DTOMaker.CSPoco
{
    public sealed class CSPocoModelScopeDomain : ModelScopeDomain
    {
        public CSPocoModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
}
