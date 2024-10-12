using System;

namespace DTOMaker.Gentime
{
    public interface ITokenMatcher<TEnum> where TEnum : struct
    {
        (int, Token<TEnum>) Match(ReadOnlyMemory<char> source);
    }
}
