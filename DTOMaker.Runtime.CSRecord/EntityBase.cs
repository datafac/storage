using System;
using System.Diagnostics.CodeAnalysis;

namespace DTOMaker.Runtime.CSRecord
{
#if NET8_0_OR_GREATER
    public abstract record EntityBase : IImmutable
    {
        protected abstract int OnGetEntityId();
        public int GetEntityId() => OnGetEntityId();

        public EntityBase() { }
        public EntityBase(object? notUsed) { }
        public bool IsFrozen => true;
    }
#endif
}
