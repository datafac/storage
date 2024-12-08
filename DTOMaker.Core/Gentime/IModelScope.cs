using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public interface IModelScope
    {
        IDictionary<string, object?> Variables { get; }
        (bool?, IModelScope[]) GetInnerScopes(string iteratorName);
    }
}
