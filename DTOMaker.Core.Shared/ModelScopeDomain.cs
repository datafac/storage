namespace DTOMaker.Gentime
{
    public abstract class ModelScopeDomain : ModelScopeBase
    {
        private readonly TargetDomain _domain;

        public ModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language)
        {
            _domain = domain;
            _tokens["DomainName"] = domain.Name;
        }
    }
}
