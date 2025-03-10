using DTOMaker.Gentime;

namespace DTOMaker.CSRecord
{
    public sealed class CSRecordModelScopeMember : ModelScopeMember
    {
        public CSRecordModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language, member)
        {
        }
    }
}
