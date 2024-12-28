using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetDomain : TargetBase
    {
        public string Name { get; }
        public ConcurrentDictionary<string, TargetEntity> Entities { get; } = new ConcurrentDictionary<string, TargetEntity>();
        public TargetDomain(string name, Location location) : base(location)
        {
            Name = name;
        }
        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics() { yield break; }
    }
}