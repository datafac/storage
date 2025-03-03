using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackMember : TargetMember
    {
        public MessagePackMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }

        private SyntaxDiagnostic? CheckEntityMemberIsNullable()
        {
            if (Kind != MemberKind.Entity) return null;
            if (MemberIsNullable) return null;
            // todo allow non-nullable entity members - need to implement Empty for abstract entitites
            return new SyntaxDiagnostic(
                    DiagnosticId.DMMP0003, "Invalid member nullability", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                    $"Entity member '{Name}' must be nullable");
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            foreach (var diagnostic1 in base.OnGetValidationDiagnostics())
            {
                yield return diagnostic1;
            }

            SyntaxDiagnostic? diagnostic2;
            if ((diagnostic2 = CheckEntityMemberIsNullable()) is not null) yield return diagnostic2;
        }

    }
}
