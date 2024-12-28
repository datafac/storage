using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.Gentime
{
    public readonly struct EntityFQN : IEquatable<EntityFQN>
    {
        // todo choose a suitable common namespace
        private static readonly EntityFQN _defaultBase = new EntityFQN("DTOMaker.Common", "EntityBase");
        public static EntityFQN DefaultBase => _defaultBase;

        private readonly string _nameSpace;
        private readonly string _shortName;
        private readonly string _fullName;

        public string NameSpace => _nameSpace;
        public string ShortName => _shortName;
        public string FullName => _fullName;

        public EntityFQN(string nameSpace, string name)
        {
            _nameSpace = nameSpace;
            _shortName = name;
            _fullName = _nameSpace + "." + _shortName;
        }

        public bool Equals(EntityFQN other)
        {
            return string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        }

        public override string ToString() => _fullName;
    }

    public abstract class TargetEntity : TargetBase
    {
        public EntityFQN EntityName { get; }
        //public string Name { get; }
        //private readonly string _nameSpace;
        //public string NameSpace => _nameSpace;

        private readonly TargetDomain _domain;
        public TargetDomain Domain => _domain;
        public ConcurrentDictionary<string, TargetMember> Members { get; } = new ConcurrentDictionary<string, TargetMember>();
        public TargetEntity(TargetDomain domain, string nameSpace, string name, Location location) : base(location)
        {
            EntityName = new EntityFQN(nameSpace, name);
            _domain = domain;
        }
        public bool HasEntityAttribute { get; set; }
        public EntityFQN BaseName { get; set; } = EntityFQN.DefaultBase;
        public TargetEntity? Base { get; set; }
        public TargetEntity[] DerivedEntities { get; set; } = [];

        public int GetClassHeight() => Base is not null ? Base.GetClassHeight() + 1 : 1;

        private SyntaxDiagnostic? CheckHasEntityAttribute()
        {
            if (HasEntityAttribute) return null;
            return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0006, "Missing [Entity] attribute", DiagnosticCategory.Design, Location, DiagnosticSeverity.Error,
                        $"[Entity] attribute is missing.");
        }

        private SyntaxDiagnostic? CheckMemberSequenceIsValid()
        {
            int expectedSequence = 1;
            foreach (var member in Members.Values.OrderBy(m => m.Sequence))
            {
                if (member.Sequence != expectedSequence)
                    return new SyntaxDiagnostic(
                        DiagnosticId.DTOM0003, "Invalid member sequence", DiagnosticCategory.Design, member.Location, DiagnosticSeverity.Error,
                        $"Expected member '{member.Name}' sequence to be {expectedSequence}, but found {member.Sequence}.");
                expectedSequence++;
            }
            return null;
        }

        protected override IEnumerable<SyntaxDiagnostic> OnGetValidationDiagnostics()
        {
            SyntaxDiagnostic? diagnostic;
            if ((diagnostic = CheckHasEntityAttribute()) is not null) yield return diagnostic;
            if ((diagnostic = CheckMemberSequenceIsValid()) is not null) yield return diagnostic;
        }
    }
}
