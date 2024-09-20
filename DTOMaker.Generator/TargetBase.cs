using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DTOMaker.Generator
{
    internal abstract class TargetBase
    {
        protected Location _location;
        public string Name { get; }
        public ConcurrentBag<SyntaxDiagnostic> SyntaxErrors { get; } = new ConcurrentBag<SyntaxDiagnostic>();
        protected TargetBase(string name, Location location)
        {
            Name = name;
            _location = location;
        }

        protected abstract IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics();
        public IEnumerable<SyntaxDiagnostic> ValidationErrors() => OnGetValidationDiagnostics();
    }
}
