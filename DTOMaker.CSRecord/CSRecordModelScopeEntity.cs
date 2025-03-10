using DTOMaker.Gentime;

namespace DTOMaker.CSRecord
{
    public sealed class CSRecordModelScopeEntity : ModelScopeEntity
    {
        public CSRecordModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language, entity)
        {
        }
    }
}
