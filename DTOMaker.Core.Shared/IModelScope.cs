using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public interface IModelScope
    {
        IReadOnlyDictionary<string, object?> Tokens { get; }
    }
}
