using DataFac.Memory;
using DataFac.Storage;
using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public abstract class EntityBase : IHasEntityId, IMemBlocksEntity, IFreezable, IEquatable<EntityBase>
    {
        #region Static Helpers
        public static async ValueTask<T> CreateEmpty<T>(IDataStore dataStore) where T: class, IMemBlocksEntity, IFreezable, new()
        {
            var empty = new T();
            await empty.Pack(dataStore);
            empty.Freeze();
            return empty;
        }
        #endregion

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
            if (!_packed) ThrowIsNotPackedException();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfNotFrozen<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
            return value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when not frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotFrozen([CallerMemberName] string? methodName = null)
        {
            if (!_frozen) ThrowIsNotFrozenException(methodName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotPackedException() => throw new InvalidOperationException($"Cannot freeze before packing.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotUnpackedException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} before unpacking.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotUnpacked([CallerMemberName] string? methodName = null)
        {
            if (_frozen && !_unpacked) ThrowIsNotUnpackedException(methodName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfUnpacked<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen && !_unpacked) ThrowIsNotUnpackedException(methodName);
            return value;
        }

        protected static T IfNotNull<T>(T? value, [CallerMemberName] string? methodName = null) where T : class
        {
            if (value is not null) return value;
            throw new InvalidOperationException($"Cannot call {methodName} when not set.");
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => HashCode.Combine<Type>(typeof(EntityBase));

        private volatile bool _packed = false;
        protected virtual ValueTask OnPack(IDataStore dataStore) => default;
        public async ValueTask Pack(IDataStore dataStore)
        {
            if (_frozen) return;
            if (_packed) return;
            await OnPack(dataStore);
            _packed = true;
            OnFreeze();
            _frozen = true;
            _unpacked = true;
        }

        private volatile bool _unpacked = false;
        protected virtual ValueTask OnUnpack(IDataStore dataStore, int depth) => default;
        public async ValueTask Unpack(IDataStore dataStore, int depth = 0)
        {
            ThrowIfNotFrozen();
            if (depth < 0) return;
            if (_unpacked) return;
            await OnUnpack(dataStore, depth);
            _unpacked = true;
        }
        public ValueTask UnpackAll(IDataStore dataStore) => Unpack(dataStore, int.MaxValue);
    }
}
