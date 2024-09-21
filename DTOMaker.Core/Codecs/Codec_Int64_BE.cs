using System;
using System.Buffers.Binary;

namespace DTOMaker.Core.Codecs
{
    public sealed class Codec_Int64_BE : Codec_Base<Int64>
    {
        private Codec_Int64_BE() { }
        public static Codec_Int64_BE Instance { get; } = new Codec_Int64_BE();
        public override Int64 OnRead(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64BigEndian(source);
        public override void OnWrite(Span<byte> target, in Int64 input) => BinaryPrimitives.WriteInt64BigEndian(target, input);
    }
}
