using System;
using System.Collections.Generic;
using System.Text;

namespace DTOMaker.Gentime
{
    public enum JsonMemberKind
    {
        Scalar, // todo rename to Native
        Vector,
        Entity,
        Binary,
        String,
    }
    public class JsonMember : Dictionary<string, object>
{
        public JsonMemberKind Kind { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsObsolete { get; set; }
    }
}
