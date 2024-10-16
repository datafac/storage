using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public interface IModelScope
    {
        ImmutableDictionary<string, object?> Tokens { get; }
        (bool?, IModelScope[]) GetInnerScopes(string iteratorName);
    }
}
