namespace DTOMaker.Gentime
{
    internal static class TargetExtensions
    {
        public static bool IsChildOf(this TargetEntity candidate, TargetEntity baseEntity)
        {
            if (ReferenceEquals(candidate, baseEntity)) return false;
            if (candidate.Base is null) return false;
            if (candidate.Base.Name == baseEntity.Name) return true;
            return candidate.Base.IsChildOf(baseEntity);
        }
    }
}
