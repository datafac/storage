using DTOMaker.Gentime;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeDomain : ModelScopeDomain
    {
        public MemBlocksModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
}
