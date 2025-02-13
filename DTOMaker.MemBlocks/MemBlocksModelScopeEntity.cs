using DTOMaker.Gentime;
using System;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeEntity : ModelScopeEntity
    {
        private static Guid ParseEntityIdAsGuid(string entityId)
        {
            if (Guid.TryParse(entityId, out Guid result)) return result;

            // todo issue warning
            // Guid not supplied - generate a random
            return Guid.NewGuid();
        }

        public MemBlocksModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity baseEntity)
            : base(parent, factory, language, baseEntity)
        {
            MemBlockEntity entity = baseEntity as MemBlockEntity
                ?? throw new ArgumentException("Expected baseEntity to be a MemBlocksEntity", nameof(baseEntity));

            _tokens["BlockLength"] = entity.BlockLength;
            _tokens["EntityId"] = ParseEntityIdAsGuid(entity.EntityIdqqq).ToString("D");
        }
    }
}
