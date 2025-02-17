using System;
using System.Security.Cryptography;
using System.Text;

namespace DTOMaker.Models
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class IdAttribute : Attribute
    {
        public readonly Guid EntityGuid;
        
        public IdAttribute(string entityGuid)
        {
            if (!Guid.TryParse(entityGuid, out Guid EntityGuid))
                EntityGuid = Guid.Empty;
        }

        private static Guid ConvertUTF8StringToGuidViaSHA1_notUsed(string value)
        {
            using var sha1 = SHA1.Create();
#if NET8_0_OR_GREATER
            Span<byte> hashSpan = stackalloc byte[20];
            Span<byte> shortSpan = stackalloc byte[100];
            if (Encoding.UTF8.TryGetBytes(value, shortSpan, out int bytesConverted))
            {
                int bytesWritten = SHA1.HashData(shortSpan, hashSpan);
            }
            else
            {
                ReadOnlySpan<byte> largeSpan = Encoding.UTF8.GetBytes(value);
                int bytesWritten = SHA1.HashData(largeSpan, hashSpan);
            }
            return new Guid(hashSpan.Slice(0, 16));
#else
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] hash = sha1.ComputeHash(valueBytes);
            return new Guid(hash.AsSpan().Slice(0, 16).ToArray());
#endif
        }

    }
}
