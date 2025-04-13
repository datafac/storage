using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class TargetEntity : TargetBase
    {
        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;

        public TypeFullName TFN { get; }
        public ConcurrentDictionary<string, TargetMember> Members { get; } = new ConcurrentDictionary<string, TargetMember>();
        public TargetEntity(TargetDomain domain, TypeFullName tfn, Location location) : base(location)
        {
            _domain = domain;
            TFN = tfn;
        }
        public int GenericTypeParams = 0;
        public bool IsGeneric => GenericTypeParams > 0;
        public TargetEntity? OpenEntity { get; set; }
        public int EntityId { get; set; }
        public bool HasEntityAttribute { get; set; }
        public TypeFullName BaseName { get; set; } = TypeFullName.DefaultBase;
        public TargetEntity? Base { get; set; }
        public TargetEntity[] DerivedEntities { get; set; } = [];

        public int GetClassHeight() => Base is not null ? Base.GetClassHeight() + 1 : 1;

        private SyntaxDiagnostic? CheckHasEntityAttribute()
        {
            if (HasEntityAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0006, "Missing [Entity] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Entity] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequenceIsValid()
        {
            int expectedSequence = 1;
            foreach (var member in Members.Values.OrderBy(m => m.Sequence))
            {
                if (member.Sequence != expectedSequence)
                    return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"Expected member '{member.Name}' sequence to be {expectedSequence}, but found {member.Sequence}.");
                expectedSequence++;
            }
            return null;
        }

        private SyntaxDiagnostic? CheckEntityIdIsValid()
        {
            if (!HasEntityAttribute) return null;

            if (EntityId > 0) return null;

            return new SyntaxDiagnostic(
                DiagnosticId.DTOM0010, "Invalid entity identifier", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                $"Entity identifier must be unique positive number. Have you forgotten the entity [Id] attribute?");
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasEntityAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequenceIsValid()) is not null) yield return diagnostic;
            if ((diagnostic = CheckEntityIdIsValid()) is not null) yield return diagnostic;
        }
    }
}
