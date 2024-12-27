using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.MessagePack
{
    internal sealed class MessagePackDomain : TargetDomain
    {
        public MessagePackDomain(string name, Location location) : base(name, location) { }

        private SyntaxDiagnostic? CheckEntityKeys()
        {
            Dictionary<int, MessagePackEntity> map = new Dictionary<int, MessagePackEntity>();
            foreach (var entity in this.Entities.Values.OfType<MessagePackEntity>())
            {
                int key = entity.EntityKey;
                if (key == 0)
                {
                    // undefined!
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMP0002, "Invalid entity key", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                        $"Entity key must be > 0.");
                }

                if (map.TryGetValue(key, out var otherEntity))
                {
                    // duplicate!
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMP0002, "Invalid entity key", DiagnosticCategory.Design, entity.Location, DiagnosticSeverity.Error,
                        $"Entity key ({key}) is already used by entity: {otherEntity.Name}");
                }

                map[key] = entity;
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
            if ((diagnostic2 = CheckEntityKeys()) is not null) yield return diagnostic2;
        }
    }
}
