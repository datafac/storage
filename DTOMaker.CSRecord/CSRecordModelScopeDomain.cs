using DTOMaker.Gentime;

namespace DTOMaker.CSRecord
{
    public sealed class CSRecordModelScopeDomain : ModelScopeDomain
    {
        public CSRecordModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
}
