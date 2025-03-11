using System;

namespace DTOMaker.Runtime.CSRecord
{
    /// <summary>
    /// Wrapper for ReadOnlyMemory\<T\> that implements IEquatable. Used to support
    /// value equality of array members in records.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ReadOnlyMemoryWrapper<T> : IEquatable<ReadOnlyMemoryWrapper<T>>
        where T: IEquatable<T>
    {
        public readonly ReadOnlyMemory<T> Memory;
        public ReadOnlyMemoryWrapper(ReadOnlyMemory<T> memory) => Memory = memory;
        public bool Equals(ReadOnlyMemoryWrapper<T> other) => other.Memory.Span.SequenceEqual(Memory.Span);
        public override bool Equals(object? obj) => obj is ReadOnlyMemoryWrapper<T> other && Equals(other);
        public override int GetHashCode()
        {
            HashCode result = new HashCode();
            result.Add(typeof(T));
            var values = Memory.Span;
            result.Add(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                result.Add(values[i]);
            }
            return result.ToHashCode();
        }
    }
}
