using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.Gentime
{
    public sealed class ModelScope_Empty : IModelScope
    {
        public ImmutableDictionary<string, TokenValue?> Tokens => ImmutableDictionary<string, TokenValue?>.Empty;

        public IEnumerable<KeyValuePair<string, object?>> TokenValues => ImmutableDictionary<string, TokenValue?>.Empty
                    .Select(t => new KeyValuePair<string, object?>(t.Key, t.Value?.Value));

        public (bool?, IModelScope[]) GetInnerScopes(string iteratorName) => (null, Array.Empty<IModelScope>());
    }
}
