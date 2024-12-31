namespace DTOMaker.Runtime
{
    /// <summary>
    /// Represents mutability of types.
    /// </summary>
    public interface IMutability
    {
        /// <summary>
        /// Returns true if the graph cannot be modified.
        /// </summary>
        bool IsFrozen { get; }
    }
}
