using DTOMaker.Gentime;

namespace DTOMaker.CSPoco
{
    public sealed class CSPocoModelScopeEntity : ModelScopeEntity
    {
        public CSPocoModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language, entity)
        {
        }
    }
}
