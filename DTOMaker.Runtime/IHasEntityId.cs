namespace DTOMaker.Runtime
{
    /// <summary>
    /// Returns the version-agnostic globally unique entity identifier.
    /// </summary>
    public interface IHasEntityId
    {
        string GetEntityId();
    }
}
