namespace DTOMaker.Gentime
{
    public enum MemberKind
    {
        Scalar, // todo rename to Native
        Vector,
        Entity,
        Binary,
        //String,
    }

    public enum BlockSize : byte
    {
        B001 = 0,   // 2^0 = 1
        B002 = 1,   // 2^1 = 2
        B004 = 2,   // 2^2 = 4
        B008 = 3,   // 2^3 = 8
        B016 = 4,   // 2^4 = 16
        //etc.
    }
}