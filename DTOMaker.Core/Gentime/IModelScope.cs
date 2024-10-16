using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public interface IModelScope
    {
        ImmutableDictionary<string, TokenValue?> Tokens { get; }
        IEnumerable<KeyValuePair<string, object?>> TokenValues { get; }
        (bool?, IModelScope[]) GetInnerScopes(string iteratorName);
    }
}
