using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class TargetDomain : TargetBase
    {
        public ConcurrentDictionary<string, TargetEntity> Entities { get; } = new ConcurrentDictionary<string, TargetEntity>();
        public TargetDomain(string name, Location location) : base(name, location) { }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            yield break;
        }
    }
}
