namespace DTOMaker.Runtime
{
    /// <summary>
    /// Represents an entity, a freezable type which is modifiable until frozen.
    /// </summary>
    public interface IEntityBase
    {
        /// <summary>
        /// Returns true if the graph cannot be modified.
        /// </summary>
        bool IsFrozen { get; }

        /// <summary>
        /// Freezes this graph including all freezable children.
        /// </summary>
        void Freeze();

        /// <summary>
        /// Returns a clone of the graph, copying the mutable parts.
        /// </summary>
        /// <returns></returns>
        IEntityBase PartCopy();
    }
}
