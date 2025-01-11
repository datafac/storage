using System;
using System.Runtime.CompilerServices;

namespace DTOMaker.Runtime.MemBlocks
{
    public abstract class EntityBase : IMemBlocksEntity, IFreezable, IEquatable<EntityBase>
    {
        public const string EntityId = "EntityBase";

        public EntityBase() { }
        public EntityBase(object? notUsed, bool frozen)
        {
            _frozen = frozen;
        }
        public EntityBase(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            _frozen = true;
        }
        private volatile bool _frozen = false;
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            _frozen = true;
            OnFreeze();
        }

        protected abstract string OnGetEntityId();
        public string GetEntityId() => OnGetEntityId();
        protected abstract int OnGetClassHeight();
        protected virtual void OnGetBuffers(ReadOnlyMemory<byte>[] buffers) { }
        public ReadOnlyMemory<ReadOnlyMemory<byte>> GetBuffers()
        {
            int height = OnGetClassHeight();
            ReadOnlyMemory<byte>[] buffers = new ReadOnlyMemory<byte>[height];
            OnGetBuffers(buffers);
            return buffers;
        }
        protected virtual void OnLoadBuffers(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) { }
        public void LoadBuffers(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            ThrowExceptionIfFrozen();
            OnLoadBuffers(buffers);
        }

        protected virtual IFreezable OnPartCopy() => throw new NotImplementedException();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowExceptionIfFrozen([CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => HashCode.Combine<Type>(typeof(EntityBase));
    }
}
