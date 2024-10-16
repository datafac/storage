using System;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Empty : IModelScope
    {
        public ImmutableDictionary<string, object?> Tokens => ImmutableDictionary<string, object?>.Empty;

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
}
