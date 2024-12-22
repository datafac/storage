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

            foreach (var entity in this.Entities.Values.OfType<MemBlockEntity>())
            {
                string id = entity.EntityId;
                if (idMap.TryGetValue(id, out var existing))
                {
                    return new SyntaxDiagnostic(
                        DiagnosticId.DMMB0011, "Duplicate entity id", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"Entity identifier '{id}' is not unique. Are you missing an [Id] attribute?");
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
