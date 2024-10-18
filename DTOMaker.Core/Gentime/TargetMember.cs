using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetMember : TargetBase
    {
        public TargetMember(string name, Location location) : base(name, location) { }
        public bool HasMemberAttribute { get; set; }
        public bool HasMemberLayoutAttribute { get; set; }
        public TargetEntity? Parent { get; set; }
        public bool IsObsolete { get; set; }
        public string? ObsoleteMessage { get; set; }
        public bool ObsoleteIsError { get; set; }
        public int Sequence { get; set; }
        public string MemberTypeName { get; set; } = "";
        public bool IsEnumType { get; set; }
        public string MemberWireTypeName { get; set; } = "";
        public int FieldOffset { get; set; }
        public int FieldLength { get; set; }
        public bool IsBigEndian { get; set; } = false;

        private SyntaxDiagnostic? CheckHasMemberAttribute()
        {
            if (HasMemberAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0007, "Missing [Member] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Member] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberType()
        {
            if (!HasMemberAttribute) return null;
            return string.IsNullOrWhiteSpace(MemberTypeName)
                ? new SyntaxDiagnostic(
                    DiagnosticId.DTOM0004, "Invalid member datatype", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"MemberType'{MemberTypeName}' must be defined")
                : null;
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
            if ((diagnostic = CheckMemberType()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
        }
    }
}
