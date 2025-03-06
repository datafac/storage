using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    public readonly struct StructureCode
    {
        private static readonly int[] _blockSizes = [0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8, 1024 * 16];
        private static int GetBlockSizeCode(int blockLength)
        {
            ReadOnlySpan<int> blockSizes = _blockSizes;
            for (byte i = 0; i < blockSizes.Length; i++)
            {
                int blockSize = blockSizes[i];
                if (blockLength <= blockSize)
                    return i;
            }

            // unsupported large block size - todo error handling
            //SyntaxErrors.Add(new SyntaxDiagnostic(
            //    DiagnosticId.DMMB0001, "Invalid block length", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
            //    $"BlockLength ({BlockLength}) is invalid. BlockLength must be a whole power of 2 between 1 and 1024."));
            return 15; // 16K
        }
        private readonly long _bits;
        private StructureCode(long bits)
        {
            _bits = bits;
        }

        public long Bits => _bits;
        public StructureCode(int classHeight, int blockLength)
        {
            // todo check class height
            int blockSizeCode = GetBlockSizeCode(blockLength);
            long init = (long)classHeight & 0x0F;
            long bits = (long)blockSizeCode << (classHeight * 4);
            _bits = (init | bits);
        }

        public StructureCode AddStructure(int classHeight, int blockLength)
        {
            int blockSizeCode = GetBlockSizeCode(blockLength);
            long bits = (long)blockSizeCode << (classHeight * 4);
            return new StructureCode(_bits | bits);
        }

    }

    internal sealed class MemBlockEntity : TargetEntity
    {
        public bool HasEntityLayoutAttribute { get; set; }
        public LayoutMethod LayoutMethod { get; set; }
        public int BlockLength { get; set; }
        public long BlockStructureCode { get; set; }

        public MemBlockEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(domain, nameSpace, name, location) { }

        private static int GetFieldLength(TargetMember member)
        {
            string typeName = member.MemberType.FullName;
            switch (typeName)
            {
                case "System.Boolean":
                case "System.Byte":
                case "System.SByte":
                    return 1;
                case "System.Int16":
                case "System.UInt16":
                case "System.Char":
                case "System.Half":
                    return 2;
                case "System.Int32":
                case "System.UInt32":
                case "System.Single":
                    return 4;
                case "System.Int64":
                case "System.UInt64":
                case "System.Double":
                    return 8;
                case "System.Int128":
                case "System.UInt128":
                case "System.Guid":
                case "System.Decimal":
                    return 16;
                // encoded as BlobIdV1
                case FullTypeName.SystemString:
                case FullTypeName.MemoryOctets:
                    return Constants.BlobIdV1Size; 
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates length for explicitly positioned members
        /// </summary>
        /// <param name="entity"></param>
        public void ExplicitLayoutMembers()
        {
            foreach (var member in this.Members.Values.OrderBy(m => m.Sequence).OfType<MemBlockMember>())
            {
                member.FieldLength = GetFieldLength(member);
            }
        }

        /// <summary>
        /// Calculates offset and length for all members in linear order
        /// </summary>
        /// <param name="entity"></param>
        public void LinearLayoutMembers()
        {
            int minBlockLength = 0;
            int nextFieldOffset = 0;

            foreach (var member in this.Members.Values.OrderBy(m => m.Sequence).OfType<MemBlockMember>())
            {
                // allocate value bytes
                int fieldLength = GetFieldLength(member);

                // adjust field/array length for fixed string and entity types
                if (member.Kind == MemberKind.Entity)
                {
                    fieldLength = Constants.BlobIdV1Size; // encoded as BlobIdV1
                }

                if (member.FixedLength != 0)
                {
                    fieldLength = member.FixedLength;
                }

                member.FieldLength = fieldLength;
                fieldLength = member.Kind == MemberKind.Vector ? fieldLength * member.ArrayCapacity : fieldLength;

                // calculate this offset
                while (fieldLength > 0 && nextFieldOffset % fieldLength != 0)
                {
                    nextFieldOffset++;
                }
                member.FieldOffset = nextFieldOffset;

                // calc next offset
                nextFieldOffset = nextFieldOffset + fieldLength;
                while (nextFieldOffset > minBlockLength)
                {
                    minBlockLength = minBlockLength == 0 ? 1 : minBlockLength * 2;
                }

            }
            this.BlockLength = minBlockLength;
        }

        public void AutoLayoutMembers()
        {
            switch (LayoutMethod)
            {
                case LayoutMethod.Explicit:
                    ExplicitLayoutMembers();
                    break;
                case LayoutMethod.Linear:
                    LinearLayoutMembers();
                    break;
            }

            // calculate structure code
            var structureCode = new StructureCode(this.GetClassHeight(), this.BlockLength);
            var parent = this.Base;
            while (parent is MemBlockEntity parentEntity)
            {
                structureCode = structureCode.AddStructure(parentEntity.GetClassHeight(), parentEntity.BlockLength);
                parent = parentEntity.Base;
            }
            this.BlockStructureCode = structureCode.Bits;
        }

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
                0 => null,
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
                        $"BlockLength ({BlockLength}) is invalid. BlockLength must be zero or a whole power of 2 between 1 and 1024.")
            };
        }

        private SyntaxDiagnostic? CheckClassHeightIsValid()
        {
            int classHeight = this.GetClassHeight();
            if (classHeight >= 1 && classHeight <= 15) return null;

            return new SyntaxDiagnostic(
                    DiagnosticId.DMMB0012, "Invalid class height", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"ClassHeight ({classHeight}) is invalid. ClassHeight must be between 1 and 15.");
        }


        private SyntaxDiagnostic? CheckLayoutMethodIsSupported()
        {
            if (!HasEntityLayoutAttribute)
                return null;

            return LayoutMethod switch
            {
                LayoutMethod.Explicit => null,
                LayoutMethod.Linear => null,
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

            if (LayoutMethod == LayoutMethod.Undefined) return null;

            foreach (var member in Members.Values.OrderBy(m => m.Sequence).OfType<MemBlockMember>())
            {
                if (member.FieldOffset < 0)
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"This member extends before the start of the block.");
                }

                if (member.FieldOffset + member.TotalLength > BlockLength)
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"This member extends beyond the end of the block.");
                }

                if (member.TotalLength > 0 && (member.FieldOffset % member.TotalLength != 0))
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0008, "Member layout issue", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"This member is incorrectly aligned. FieldOffset ({member.FieldOffset}) modulo total length ({member.TotalLength}) must be 0.");
                }

                // check value bytes layout
                for (var i = 0; i < member.TotalLength; i++)
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

        private SyntaxDiagnostic? CheckEntityIdIsGuid()
        {
            if (!HasEntityAttribute) return null;

            if (!Guid.TryParse(this.EntityId, out Guid entityGuid))
                return new SyntaxDiagnostic(
                            DiagnosticId.DMMB0011, "Invalid entity identifier", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                            $"Entity identifier must be a GUID. Have you forgotten the entity [Id] attribute?");

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
            if ((diagnostic2 = CheckEntityIdIsGuid()) is not null) yield return diagnostic2;
            if ((diagnostic2 = CheckClassHeightIsValid()) is not null) yield return diagnostic2;
        }
    }
}