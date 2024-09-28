using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public abstract class TargetMember : TargetBase
    {
        public TargetMember(string name, Location location) : base(name, location) { }
        public int Sequence { get; set; }
        public string MemberType { get; set; } = "";
        public int? FieldOffset { get; set; }
        public int? FieldLength { get; set; }
        public bool IsBigEndian { get; set; } = false;
        // todo move to derived
        public string CodecTypeName => $"DTOMaker.Runtime.Codec_{MemberType}_{(IsBigEndian ? "BE" : "LE")}";

        // todo move to derived
        private SyntaxDiagnostic? CheckFieldOffset()
        {
            return FieldOffset switch
            {
                null => null, // todo not allowed when required
                >= 0 => null,
                _ => new SyntaxDiagnostic(Location, DiagnosticSeverity.Error, $"FieldOffset ({FieldOffset}) must be >= 0")
            };
        }

        // todo move to derived
        private SyntaxDiagnostic? CheckFieldLength()
        {
            return FieldLength switch
            {
                null => null, // todo not allowed when required
                > 0 => null,
                _ => new SyntaxDiagnostic(Location, DiagnosticSeverity.Error, $"FieldLength ({FieldLength}) must be > 0")
            };
        }

        private SyntaxDiagnostic? CheckMemberType()
        {
            return string.IsNullOrWhiteSpace(MemberType)
                ? new SyntaxDiagnostic(Location, DiagnosticSeverity.Error, $"MemberType'{MemberType}' must be defined")
                : null;
        }

        private SyntaxDiagnostic? CheckMemberSequence()
        {
            return Sequence <= 0
                ? new SyntaxDiagnostic(Location, DiagnosticSeverity.Error, $"Sequence ({Sequence}) must be > 0")
                : null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckMemberType()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
            // todo move to derived
            if ((diagnostic = CheckFieldOffset()) is not null) yield return diagnostic;
            if ((diagnostic = CheckFieldLength()) is not null) yield return diagnostic;
        }
    }
}
