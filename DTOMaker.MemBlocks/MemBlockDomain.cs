using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.MemBlocks
{
    internal sealed class MemBlockDomain : TargetDomain
    {
        public MemBlockDomain(string name, Location location) : base(name, location) { }

        private SyntaxDiagnostic? CheckEntityIdsAreUnique()
        {
            Dictionary<string, MemBlockEntity> idMap = new Dictionary<string, MemBlockEntity>();

            foreach (var entity in this.Entities.Values.OfType<MemBlockEntity>().OrderBy(e => e.EntityName.FullName))
            {
                string id = entity.EntityId;
                if (idMap.TryGetValue(id, out var otherEntity))
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0011, "Duplicate entity id", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Entity id ({id}) is already used by entity: {otherEntity.EntityName}");
                }
                idMap[id] = entity;
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
            if ((diagnostic2 = CheckEntityIdsAreUnique()) is not null) yield return diagnostic2;
        }
    }
}
