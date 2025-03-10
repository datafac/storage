using Microsoft.CodeAnalysis;
using System.Linq;

namespace DTOMaker.Gentime
{
    public abstract class ModelScopeEntity : ModelScopeBase
    {
        protected readonly TargetEntity _entity;
        public readonly int DerivedEntityCount;
        public readonly int ClassHeight;

        public ModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language)
        {
            DerivedEntityCount = entity.DerivedEntities.Length;
            ClassHeight = entity.GetClassHeight();

            _entity = entity;
            _tokens["NameSpace"] = entity.EntityName.NameSpace;
            _tokens["EntityName"] = entity.EntityName.ShortName;
            _tokens["AbstractEntityName"] = entity.EntityName.ShortName;
            _tokens["ConcreteEntityName"] = entity.EntityName.ShortName;
            _tokens["EntityId"] = entity.EntityId;
            _tokens["BaseName"] = entity.Base?.EntityName.ShortName ?? TypeFullName.DefaultBase.ShortName;
            _tokens["BaseNameSpace"] = entity.Base?.EntityName.NameSpace ?? TypeFullName.DefaultBase.NameSpace;
            _tokens["BaseFullName"] = entity.Base?.EntityName.FullName ?? TypeFullName.DefaultBase.FullName;
            _tokens["ClassHeight"] = ClassHeight;
            _tokens["DerivedEntityCount"] = DerivedEntityCount;
        }

        private static bool IsDerivedFrom(TargetEntity candidate, TargetEntity parent)
        {
            if (ReferenceEquals(candidate, parent)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.EntityName.Equals(parent.EntityName)) return true;
            return IsDerivedFrom(candidate.Base, parent);
        }

        public ModelScopeMember[] Members
        {
            get
            {
                return _entity.Members.Values
                            .OrderBy(m => m.Sequence)
                            .Select(m => _factory.CreateMember(this, _factory, _language, m))
                            .ToArray();
            }
        }

        public ModelScopeEntity[] DerivedEntities
        {
            get
            {
                return _entity.DerivedEntities
                        .OrderBy(e => e.EntityName.FullName)
                        .Select(e => _factory.CreateEntity(this, _factory, _language, e))
                        .ToArray();
            }
        }
    }
}
