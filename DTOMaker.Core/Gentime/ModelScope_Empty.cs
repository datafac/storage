using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Empty : IModelScope
    {
        public ImmutableDictionary<string, object?> Tokens => ImmutableDictionary<string, object?>.Empty;

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
}
