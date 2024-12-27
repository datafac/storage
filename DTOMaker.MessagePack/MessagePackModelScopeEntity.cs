using DTOMaker.Gentime;
using System;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackModelScopeEntity : ModelScopeEntity
    {
        public MessagePackModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity baseEntity)
            : base(parent, factory, language, baseEntity)
        {
            MessagePackEntity entity = baseEntity as MessagePackEntity
                ?? throw new ArgumentException("Expected baseEntity to be a MessagePackEntity", nameof(baseEntity));
            _variables["EntityKey"] = entity.EntityKey;
        }
    }
}
