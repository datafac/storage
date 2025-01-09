using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetMember : TargetBase
    {
        private readonly string _name;
        private readonly TargetEntity _entity;

        public string Name => _name;
        public TargetEntity Entity => _entity;
        public TargetMember(TargetEntity entity, string name, Location location) : base(location)
        {
            _entity = entity;
            _name = name;
        }
        public bool HasMemberAttribute { get; set; }
        public TypeFullName MemberType { get; set; }
        public bool MemberIsValueType { get; set; }
        public bool MemberIsReferenceType { get; set; }
        public bool MemberIsNullable { get; set; }
        public bool IsObsolete { get; set; }
        public string ObsoleteMessage { get; set; } = "";
        public bool ObsoleteIsError { get; set; }
        public int Sequence { get; set; }
        public bool MemberIsVector { get; set; }
        public bool MemberIsEntity { get; set; }

        private SyntaxDiagnostic? CheckHasMemberAttribute()
        {
            if (HasMemberAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0007, "Missing [Member] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Member] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequence()
        {
            if (!HasMemberAttribute) return null;
            return Sequence <= 0
                ? new SyntaxDiagnostic(
                    DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"Sequence ({Sequence}) must be > 0")
                : null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasMemberAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
        }
    }
}
