namespace DTOMaker.Gentime
{
    // These types mimic those in DTOMaker.Models and must be kept in sync.

    public readonly struct DomainAttribute { }
    public readonly struct EntityAttribute { }
    public readonly struct MemberAttribute { }


    // todo move to MemBlocks
    public readonly struct EntityLayoutAttribute { }
    public readonly struct MemberLayoutAttribute { }

    public enum LayoutMethod : int
    {
        Undefined = 0,
        Explicit = 1,
        SequentialV1 = 2,
    }
}
