using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockMember : TargetMember
    {
        public MemBlockMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public MemBlockMember(TargetEntity entity, MemBlockMember source) : base(entity, source)
        {
            HasOffsetAttribute = source.HasOffsetAttribute;
            FixedLength = source.FixedLength;
            ArrayCapacity = source.ArrayCapacity;
            FieldOffset = source.FieldOffset;
            FieldLength = source.FieldLength;
            IsBigEndian = source.IsBigEndian;
        }

        public LayoutMethod LayoutMethod => (Entity as MemBlockEntity)?.LayoutMethod ?? LayoutMethod.Undefined;

        public bool HasOffsetAttribute { get; set; }
        public int FixedLength { get; set; }
        public bool IsFixedLength => FixedLength != 0;
        public int ArrayCapacity { get; set; }
        public int FieldOffset { get; set; }
        public int FieldLength { get; set; }
        public int TotalLength => (Kind == MemberKind.Vector) ? FieldLength * ArrayCapacity : FieldLength;
        public bool IsBigEndian { get; set; } = false;

        private SyntaxDiagnostic? CheckMemberIsNotNullable()
        {
            if (Kind == MemberKind.Entity) return null;
            if (Kind == MemberKind.Binary) return null;
            if (Kind == MemberKind.String) return null;
            if (!MemberIsNullable) return null;

            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0007, "Unsupported member type", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Nullable type '{MemberType}?' is not supported.");
        }

        private SyntaxDiagnostic? CheckHasOffsetAttribute()
        {
            if (LayoutMethod == LayoutMethod.Undefined) return null;
            if (LayoutMethod == LayoutMethod.Linear) return null;
            //if (LayoutMethod == LayoutMethod.Compact) return null;

            if (HasOffsetAttribute) return null;

            return (SyntaxDiagnostic?)new SyntaxDiagnostic(
                     DiagnosticId.DMMB0006, "Missing [Offset] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                     $"[Offset] attribute is missing. This is required for {LayoutMethod} layout method.");
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

        private static bool IsPowerOf2(int value, int minimum = 1, int maximum = 1024)
        {
            if (value < minimum) return false;
            if (value > maximum) return false;
            int comparand = 1;
            while (true)
            {
                if (comparand > value) return false;
                if (value == comparand) return true;
                comparand = comparand * 2;
            }
        }

        private SyntaxDiagnostic? CheckFixedLengthIsValid()
        {
            if (MemberType.FullName != FullTypeName.SystemString
                && MemberType.FullName != FullTypeName.MemoryOctetsqqq) return null;
            if (FixedLength == 0) return null;
            if (IsPowerOf2(FixedLength, 4, 1024)) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0009, "Invalid length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FixedLength ({FixedLength}) is invalid. FixedLength must be a whole power of 2 between 4 and 1024.");
        }

        private SyntaxDiagnostic? CheckArrayCapacityIsValid()
        {
            if (Kind != MemberKind.Vector) return null;
            if (IsPowerOf2(ArrayCapacity, 1, 1024)) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0009, "Invalid array capacity", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"ArrayCapacity ({ArrayCapacity}) is invalid. ArrayCapacity must be a whole power of 2 between 1 and 1024.");
        }

        private SyntaxDiagnostic? CheckFieldLengthIsValid()
        {
            if (IsPowerOf2(FieldLength, 1, 1024)) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0003, "Invalid field length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"FieldLength ({FieldLength}) is invalid. FieldLength must be a whole power of 2 between 1 and 1024.");
        }

        private SyntaxDiagnostic? CheckTotalLengthIsValid()
        {
            if (Kind != MemberKind.Vector) return null;
            int totalLength = FieldLength * ArrayCapacity;
            if (IsPowerOf2(totalLength, 1, 1024)) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0009, "Invalid total length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Total length ({totalLength}) is invalid. Total length must be a whole power of 2 between 1 and 1024.");
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            foreach (var diagnostic1 in base.OnGetValidationDiagnostics())
            {
                yield return diagnostic1;
            }

            SyntaxDiagnostic? diagnostic2;
            if ((diagnostic2 = CheckMemberIsNotNullable()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckHasOffsetAttribute()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldOffsetIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFieldLengthIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckFixedLengthIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckArrayCapacityIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckTotalLengthIsValid()) is not null) yield return diagnostic2;
        }


    }
}
