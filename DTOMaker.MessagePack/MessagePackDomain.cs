using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackDomain : TargetDomain
    {
        public MessagePackDomain(string name, Location location) : base(name, location) { }

        private SyntaxDiagnostic? CheckEntityTags()
        {
            Dictionary<int, MessagePackEntity> map = new Dictionary<int, MessagePackEntity>();
            foreach (var entity in this.Entities.Values.OfType<MessagePackEntity>())
            {
                int tag = entity.EntityTag;
                if (tag == 0)
                {
                    // undefined!
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMP0002, "Invalid entity tag", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                        $"Entity tag must be > 0.");
                }

                if (map.TryGetValue(tag, out var otherEntity))
                {
                    // duplicate!
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMP0002, "Invalid entity tag", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                        $"Entity tag ({tag}) is already used by entity: {otherEntity.Name}");
                }

                map[tag] = entity;
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
            if ((diagnostic2 = CheckEntityTags()) is not null) yield return diagnostic2;
        }
    }
}
