using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetMember : TargetBase
    {
        public TargetMember(string name, Location location) : base(name, location) { }
        public bool HasMemberAttribute { get; set; }
        public bool HasMemberLayoutAttribute { get; set; }
        public int Sequence { get; set; }
        public string MemberType { get; set; } = "";
        public int? FieldOffset { get; set; }
        public int? FieldLength { get; set; }
        public bool IsBigEndian { get; set; } = false;

        private SyntaxDiagnostic? CheckMemberType()
        {
            return string.IsNullOrWhiteSpace(MemberType)
                ? new SyntaxDiagnostic(
                    DiagnosticId.DTOM0004, "Invalid member datatype", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"MemberType'{MemberType}' must be defined")
                : null;
        }

        private SyntaxDiagnostic? CheckHasMemberAttribute()
        {
            if (HasMemberAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0007, "Missing [Member] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Member] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequence()
        {
            return Sequence <= 0
                ? new SyntaxDiagnostic(
                    DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"Sequence ({Sequence}) must be > 0")
                : null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckMemberType()) is not null) yield return diagnostic;
            if ((diagnostic = CheckHasMemberAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
        }
    }
}
