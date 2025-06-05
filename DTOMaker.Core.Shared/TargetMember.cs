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
        public TargetMember(TargetEntity entity, TargetMember source) : base(source.Location)
        {
            _entity = entity;
            _name = source.Name;
            HasMemberAttribute = source.HasMemberAttribute;
            Sequence = source.Sequence;
            Kind = source.Kind;
            MemberIsNullable = source.MemberIsNullable;
            MemberType = source.MemberType;
            IsObsolete = source.IsObsolete;
            ObsoleteMessage = source.ObsoleteMessage;
            ObsoleteIsError = source.ObsoleteIsError;
        }

        public bool HasMemberAttribute { get; set; }
        public TypeFullName MemberType { get; set; }
        public bool MemberIsNullable { get; set; }
        public bool IsObsolete { get; set; }
        public string ObsoleteMessage { get; set; } = "";
        public bool ObsoleteIsError { get; set; }
        public int Sequence { get; set; }
        public MemberKind Kind { get; set; }

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

        private SyntaxDiagnostic? CheckMemberKind()
        {
            if (!HasMemberAttribute) return null;
            return Kind switch
            {
                MemberKind.Native => null,
                MemberKind.String => null,
                MemberKind.Binary => null,
                MemberKind.Entity => null,
                MemberKind.Vector => null,
                _ => new SyntaxDiagnostic(
                    DiagnosticId.DTOM0004, "Invalid member datatype", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"Member '{Name}' has invalid data type '{MemberType}'.")

            };
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasMemberAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequence()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberKind()) is not null) yield return diagnostic;
        }
    }
}
