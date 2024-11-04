using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public interface IModelScope
    {
        IDictionary<string, object?> Variables { get; }
        (bool?, IModelScope[]) GetInnerScopes(string iteratorName);
    }
}
