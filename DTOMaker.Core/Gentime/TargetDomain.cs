using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace DTOMaker.Gentime
{
    public abstract class TargetDomain : TargetBase
    {
        public ConcurrentDictionary<string, TargetEntity> Entities { get; } = new ConcurrentDictionary<string, TargetEntity>();
        public TargetDomain(string name, Location location) : base(name, location) { }
    }
}
