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

        // todo move to derived
        private SyntaxDiagnostic? CheckBlockSizeIsValid()
        {
            return BlockSize switch
            {
                null => null,
                1 => null,
                2 => null,
                4 => null,
                8 => null,
                16 => null,
                32 => null,
                64 => null,
                128 => null,
                256 => null,
                512 => null,
                1024 => null,
                _ => new SyntaxDiagnostic(Location, DiagnosticSeverity.Error,
                    $"BlockSize ({BlockSize}) is invalid. BlockSize must be a power of 2, and between 1 and 1024")
            };
        }

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
            if ((diagnostic = CheckBlockSizeIsValid()) is not null) yield return diagnostic;
        }
    }
}
