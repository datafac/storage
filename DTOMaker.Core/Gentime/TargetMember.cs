using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class TargetMember : TargetBase
    {
        public TargetMember(string name, Location location) : base(name, location) { }
        public int Sequence { get; set; }
        public string MemberType { get; set; } = "";
        public int? FieldOffset { get; set; }
        public int? FieldLength { get; set; }
        public bool IsBigEndian { get; set; } = false;
        public string CodecTypeName => $"DTOMaker.Runtime.Codec_{MemberType}_{(IsBigEndian ? "BE" : "LE")}";

        private bool FieldOffsetIsValid()
        {
            return FieldOffset switch
            {
                null => true,
                _ => FieldOffset >= 0
            };
        }

        private bool FieldLengthIsValid()
        {
            return FieldLength switch
            {
                null => true,
                _ => FieldLength > 0
            };
        }

        public bool CanEmit()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(MemberType)
                && Sequence > 0
                && FieldOffsetIsValid()
                && FieldLengthIsValid();
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            if (string.IsNullOrWhiteSpace(MemberType))
            {
                yield return new SyntaxDiagnostic(_location, DiagnosticSeverity.Error, $"MemberType'{MemberType}' must be defined");
            }
            if (Sequence <= 0)
            {
                yield return new SyntaxDiagnostic(_location, DiagnosticSeverity.Error, $"Sequence ({Sequence}) must be > 0");
            }
            if (!FieldOffsetIsValid())
            {
                yield return new SyntaxDiagnostic(_location, DiagnosticSeverity.Error, $"FieldOffset ({FieldOffset}) must be >= 0");
            }
            if (!FieldLengthIsValid())
            {
                yield return new SyntaxDiagnostic(_location, DiagnosticSeverity.Error, $"FieldLength ({FieldLength}) must be > 0");
            }
            yield break; // todo
        }
    }
}
