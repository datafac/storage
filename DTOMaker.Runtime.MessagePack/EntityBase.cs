using MessagePack;
using System;
using System.Runtime.CompilerServices;

namespace DTOMaker.Runtime.MessagePack
{
    [MessagePackObject]
    public abstract class EntityBase : IHasEntityId, IFreezable, IEquatable<EntityBase>
    {
        public const int EntityKey = 0;
        protected abstract string OnGetEntityId();
        public string GetEntityId() => OnGetEntityId();

        public EntityBase() { }
        public EntityBase(object? notUsed) { }

        [IgnoreMember]
        private volatile bool _frozen;
        [IgnoreMember]
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            OnFreeze();
            _frozen = true;
        }
        protected abstract IFreezable OnPartCopy();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfNotFrozen<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfFrozen([CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => HashCode.Combine<Type>(typeof(EntityBase));

        protected static bool BinaryValuesAreEqual(ReadOnlyMemory<byte>? left, ReadOnlyMemory<byte>? right)
        {
            if (left is null) return (right is null);
            if (right is null) return false;
            return left.Value.Span.SequenceEqual(right.Value.Span);
        }

    }
}
