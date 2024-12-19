using DTOMaker.Gentime;
using System;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlocksModelScopeEntity : ModelScopeEntity
    {
        public MemBlocksModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity baseEntity)
            : base(parent, factory, language, baseEntity)
        {
            MemBlockEntity entity = baseEntity as MemBlockEntity
                ?? throw new ArgumentException("Expected baseEntity to be a MemBlocksEntity", nameof(baseEntity));
        }
    }
}
