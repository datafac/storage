using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetBase
    {
        public Location Location { get; }
        public ConcurrentBag<SyntaxDiagnostic> SyntaxErrors { get; } = new ConcurrentBag<SyntaxDiagnostic>();
        protected TargetBase(Location location)
        {
            Location = location;
        }

        protected abstract IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics();
        public IEnumerable<SyntaxDiagnostic> ValidationErrors() => OnGetValidationDiagnostics();
    }
}
