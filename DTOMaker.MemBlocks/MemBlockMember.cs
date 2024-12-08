using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockMember : TargetMember
    {
        public MemBlockMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }

        public LayoutMethod LayoutMethod => (Entity as MemBlockEntity)?.LayoutMethod ?? LayoutMethod.Undefined;

        private SyntaxDiagnostic? CheckMemberType()
        {
            return MemberTypeName switch
            {
                "Boolean" => null,
                "SByte" => null,
                "Byte" => null,
                "Int16" => null,
                "UInt16" => null,
                "Char" => null,
                "Int32" => null,
                "UInt32" => null,
                "Int64" => null,
                "UInt64" => null,
                "Half" => null,
                "Single" => null,
                "Double" => null,
                "Int128" => null,
                "UInt128" => null,
                "Decimal" => null,
                "Guid" => null,
                _ => new SyntaxDiagnostic(
                    DiagnosticId.DMMB0007, "Unsupported member datatype", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"MemberType '{MemberTypeName}' not supported")
            };
        }

        private SyntaxDiagnostic? CheckMemberIsNotNullable()
        {
            if (!MemberIsNullable) return null;

            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0007, "Unsupported member type", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Nullable type '{MemberTypeName}?' is not supported.");
        }

        private SyntaxDiagnostic? CheckHasMemberLayoutAttribute()
        {
            if (LayoutMethod == LayoutMethod.SequentialV1)
                return null;

            if (HasMemberLayoutAttribute) return null;

            return (SyntaxDiagnostic?)new SyntaxDiagnostic(
                     DiagnosticId.DMMB0006, "Missing [MemberLayout] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                     "[MemberLayout] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckFieldOffsetIsValid()
        {
            return FieldOffset switch
            {
                >= 0 => null,
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0002, "Invalid field offset", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FieldOffset ({FieldOffset}) must be >= 0")
            };
        }

        private SyntaxDiagnostic? CheckArrayLengthIsValid()
        {
            if (!MemberIsArray) return null;
            return ArrayLength switch
            {
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
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0009, "Invalid array length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"ArrayLength ({ArrayLength}) is invalid. ArrayLength must be a whole power of 2 between 1 and 1024.")
            };
        }

        private SyntaxDiagnostic? CheckFieldLengthIsValid()
        {
            return FieldLength switch
            {
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
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0003, "Invalid field length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FieldLength ({FieldLength}) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.")
            };
        }

        private SyntaxDiagnostic? CheckTotalLengthIsValid()
        {
            if (!MemberIsArray) return null;
            int totalLength = FieldLength * ArrayLength;
            return totalLength switch
            {
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
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0009, "Invalid array length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Total length ({totalLength}) is invalid. Total length must be a whole power of 2 between 1 and 1024.")
            };
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            foreach (var diagnostic1 in base.OnGetValidationDiagnostics())
            {
                yield return diagnostic1;
            }

            SyntaxDiagnostic? diagnostic2;
            if ((diagnostic2 = CheckMemberType()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckMemberIsNotNullable()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckHasMemberLayoutAttribute()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldOffsetIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldLengthIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckArrayLengthIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckTotalLengthIsValid()) is not null) yield return diagnostic2;
        }


    }
}
