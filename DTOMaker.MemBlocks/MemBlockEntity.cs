using DTOMaker.Gentime;
using DTOMaker.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockEntity : TargetEntity
    {
        public MemBlockEntity(string name, Location location) : base(name, location) { }

        private SyntaxDiagnostic? CheckHasEntityLayoutAttribute()
        {
            return !HasEntityLayoutAttribute
                ? new SyntaxDiagnostic(
                        DiagnosticId.DMMB0005, "Missing [EntityLayout] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[EntityLayout] attribute is missing.")
                : null;
        }

        private SyntaxDiagnostic? CheckBlockSizeIsValid()
        {
            if (!HasEntityLayoutAttribute)
                return null;

            if (LayoutMethod != LayoutMethod.Explicit) 
                return null;

            return BlockLength switch
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
                        DiagnosticId.DMMB0001, "Invalid block length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"BlockLength ({BlockLength}) is invalid. BlockLength must be a whole power of 2 between 1 and 1024")
            };
        }

        private SyntaxDiagnostic? CheckLayoutMethodIsSupported()
        {
            if (!HasEntityLayoutAttribute)
                return null;

            return LayoutMethod switch
            {
                LayoutMethod.Explicit => null,
                LayoutMethod.SequentialV1 => null,
                LayoutMethod.Undefined => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0004, "Invalid layout method", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"LayoutMethod is not defined."),
                _ => new SyntaxDiagnostic(
                        DiagnosticId.DMMB0004, "Invalid layout method", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"LayoutMethod ({LayoutMethod}) is not supported.")
            };
        }

        private SyntaxDiagnostic? CheckMemberLayoutHasNoOverlaps()
        {
            // memory map of every byte in the entity block
            int[] memberMap = new int[BlockLength];

            foreach (var member in Members.Values.OrderBy(m => m.Sequence))
            {
                if (member.FieldOffset < 0)
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"This member extends before the start of the block.");
                }

                if (member.FieldOffset + member.FieldLength > BlockLength)
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"This member extends beyond the end of the block.");
                }

                for (var i = 0; i < member.FieldLength; i++)
                {
                    int offset = member.FieldOffset + i;
                    if (memberMap[offset] != 0)
                    {
                        int conflictSequence = memberMap[offset];
                        return new SyntaxDiagnostic(
                            DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                            $"This member overlaps memory assigned to another member (sequence {conflictSequence}).");
                    }
                    else
                    {
                        // not assigned
                        memberMap[offset] = member.Sequence;
                    }
                }
            }

            return null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            foreach (var diagnostic1 in base.OnGetValidationDiagnostics())
            {
                yield return diagnostic1;
            }

            SyntaxDiagnostic? diagnostic2;
            if ((diagnostic2 = CheckHasEntityLayoutAttribute()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckLayoutMethodIsSupported()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckBlockSizeIsValid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckMemberLayoutHasNoOverlaps()) is not null) yield return diagnostic2;
        }
    }
}