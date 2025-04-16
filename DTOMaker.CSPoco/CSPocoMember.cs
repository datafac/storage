using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace DTOMaker.CSPoco
{
    internal sealed class CSPocoMember : TargetMember
    {
        public CSPocoMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public CSPocoMember(TargetEntity entity, CSPocoMember source) : base(entity, source) { }
    }
}
