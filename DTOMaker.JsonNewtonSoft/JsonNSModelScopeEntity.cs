using DTOMaker.Gentime;

namespace DTOMaker.JsonNewtonSoft
{
    public sealed class JsonNSModelScopeEntity : ModelScopeEntity
    {
        public JsonNSModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language, entity)
        {
        }
    }
}
