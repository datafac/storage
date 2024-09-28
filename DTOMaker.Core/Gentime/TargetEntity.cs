using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class TargetEntity : TargetBase
    {
        public ConcurrentDictionary<string, TargetMember> Members { get; } = new ConcurrentDictionary<string, TargetMember>();
        public TargetEntity(string name, Location location) : base(name, location) { }
        public int? BlockSize { get; set; }

        private SyntaxDiagnostic? CheckMemberSequenceIsValid()
        {
            int expectedSequence = 1;
            foreach (var member in Members.Values.OrderBy(m => m.Sequence))
            {
                if (member.Sequence != expectedSequence)
                    return new SyntaxDiagnostic(member.Location, DiagnosticSeverity.Error,
                        $"Expected member '{member.Name}' sequence to be {expectedSequence}, but found {member.Sequence}.");
                expectedSequence++;
            }
            return null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckMemberSequenceIsValid()) is not null) yield return diagnostic;
        }
    }
}
