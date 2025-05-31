namespace DTOMaker.Gentime
{
    public enum MemberKind
    {
        Unknown,
        Native,
        Entity,
        Binary,
        String,
        Generic,
        Vector, // todo replace with Rank (0=scalar, 1=vector, 2=matrix, 3=tensor, etc.)
    }
}