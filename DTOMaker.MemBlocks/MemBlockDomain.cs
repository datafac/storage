using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockDomain : TargetDomain
    {
        public MemBlockDomain(string name, Location location) : base(name, location) { }

        // no need yet to override OnGetValidationDiagnostics()
    }
}
