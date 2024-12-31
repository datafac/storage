namespace DTOMaker.Runtime
{
    /// <summary>
    /// Represents a mutable (always modifiable) type.
    /// </summary>
    public interface IMutable : IMutability
    {
        /// <summary>
        /// Returns a full clone of the entire graph.
        /// </summary>
        /// <returns></returns>
        IMutable FullCopy();
    }
}
