namespace DTOMaker.Runtime
{
    public interface IMutability
    {
        /// <summary>
        /// Returns true if the graph cannot be modified.
        /// </summary>
        bool IsFrozen();
    }
}
