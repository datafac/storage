using DataFac.Memory;
using DataFac.Storage;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public abstract class EntityBase : IHasEntityId, IMemBlocksEntity, IFreezable, IEquatable<EntityBase>
    {
        public const string EntityId = "EntityBase";
        protected abstract string OnGetEntityId();
        public string GetEntityId() => OnGetEntityId();

        public EntityBase() { }
        public EntityBase(object? notUsed)
        {
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
            OnFreeze();
            _frozen = true;
        }

        protected abstract int OnGetClassHeight();
        protected virtual void OnGetBuffers(ReadOnlyMemory<byte>[] buffers) { }
        public ReadOnlyMemory<byte> GetBuffer()
        {
            string entityId = OnGetEntityId();
            int height = OnGetClassHeight();
            var buffers = new ReadOnlyMemory<byte>[height];
            OnGetBuffers(buffers);
            return DataFac.MemBlocks.Protocol.CombineBuffers(entityId, buffers);
        }

        protected virtual void OnLoadBuffers(ReadOnlyMemory<byte>[] buffers) { }
        public void LoadBuffer(ReadOnlyMemory<byte> buffer)
        {
            ThrowIfFrozen();
            var buffers = DataFac.MemBlocks.Protocol.SplitBuffers(buffer);
            OnLoadBuffers(buffers);
        }

        protected virtual IFreezable OnPartCopy() => throw new NotImplementedException();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfFrozen([CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when not frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotFrozen([CallerMemberName] string? methodName = null)
        {
            if (!_frozen) ThrowIsNotFrozenException(methodName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotPackedException(string? methodName) => throw new InvalidOperationException($"Cannot freeze before packing: {methodName}.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotPacked(bool packed, [CallerMemberName] string? methodName = null)
        {
            if (!packed) ThrowIsNotPackedException(methodName);
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => HashCode.Combine<Type>(typeof(EntityBase));

        protected virtual ValueTask OnPack(IDataStore dataStore) => default;
        public ValueTask Pack(IDataStore dataStore) => _frozen ? default : OnPack(dataStore);

        protected virtual ValueTask OnUnpack(IDataStore dataStore) => default;
        public ValueTask Unpack(IDataStore dataStore)
        {
            ThrowIfNotFrozen();
            return OnUnpack(dataStore);
        }
    }
}
