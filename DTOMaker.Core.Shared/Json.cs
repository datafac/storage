using System;
using System.Linq;

namespace DTOMaker.Gentime.Json
{
    public sealed class JsonMember
    {
        public int Sequence { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeFullName { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsObsolete { get; set; }
        public bool ObsoleteIsError { get; set; }
        public string ObsoleteMessage { get; set; } = string.Empty;
    }
    public sealed class JsonEntity
    {
        public int EntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseName { get; set; } = string.Empty;
        public JsonMember[] Members { get; set; } = Array.Empty<JsonMember>();
    }
    public sealed class JsonModel
    {
        public JsonEntity[] Entities { get; set; } = Array.Empty<JsonEntity>();
    }
    public static class JsonHelpers
    {
        public static JsonMember ToJson(this TargetMember member)
        {
            return new JsonMember()
            {
                Sequence = member.Sequence,
                Name = member.Name,
                //Kind = member.Kind,
                TypeFullName = member.MemberType.FullName,
                IsNullable = member.MemberIsNullable,
                IsObsolete = member.IsObsolete,
                ObsoleteIsError = member.ObsoleteIsError,
                ObsoleteMessage = member.ObsoleteMessage,
            };
        }

        public static JsonEntity ToJson(this TargetEntity entity)
        {
            return new JsonEntity()
            {
                EntityId = entity.EntityId,
                Name = entity.TFN.FullName,
                BaseName = entity.BaseName.FullName,
                Members = entity.Members.Values
                    .OrderBy(m => m.Sequence)
                    .Select(m => m.ToJson())
                    .ToArray(),
            };
        }
    }
}
