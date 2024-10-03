using DTOMaker.Gentime;
using DTOMaker.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockMember : TargetMember
    {
        public MemBlockMember(string name, Location location) : base(name, location)
        {
        }

        public LayoutMethod LayoutMethod => Parent?.LayoutMethod ?? LayoutMethod.Undefined;

        public string CodecTypeName => $"DTOMaker.Runtime.Codec_{MemberType}_{(IsBigEndian ? "BE" : "LE")}";

        private SyntaxDiagnostic? CheckHasMemberLayoutAttribute()
        {
            if (LayoutMethod == LayoutMethod.SequentialV1)
                return null;

            return !HasMemberLayoutAttribute
                ? new SyntaxDiagnostic(
                        DiagnosticId.DMMB0006, "Missing [MemberLayout] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[MemberLayout] attribute is missing.")
                : null;
        }
        private SyntaxDiagnostic? CheckFieldOffsetIsValid()
        {
            if (!HasMemberLayoutAttribute)
                return null;

            return FieldOffset switch
            {
                >= 0 => null,
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0002, "Invalid field offset", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FieldOffset ({FieldOffset}) must be >= 0")
            };
        }

        private SyntaxDiagnostic? CheckFieldLengthIsValid()
        {
            if (!HasMemberLayoutAttribute)
                return null;

            // todo? field length should match datatype and cardinality
            return FieldLength switch
            {
                > 0 => null,
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0003, "Invalid field length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FieldLength ({FieldLength}) must be > 0")
            };
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            foreach (var diagnostic1 in base.OnGetValidationDiagnostics())
            {
                yield return diagnostic1;
            }

            SyntaxDiagnostic? diagnostic2;
            if ((diagnostic2 = CheckHasMemberLayoutAttribute()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldOffsetIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldLengthIsValid()) is not null) yield return diagnostic2;
        }


    }
}
