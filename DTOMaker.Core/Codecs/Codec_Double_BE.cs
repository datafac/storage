using System;
using System.Buffers.Binary;

namespace DTOMaker.Core.Codecs
{
    public sealed class Codec_Double_BE : Codec_Base<double>
    {
        private Codec_Double_BE() { }
        public static Codec_Double_BE Instance { get; } = new Codec_Double_BE();
        public override double OnRead(ReadOnlySpan<byte> source) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(source));
        public override void OnWrite(Span<byte> target, in double input) => BinaryPrimitives.WriteInt64BigEndian(target, BitConverter.DoubleToInt64Bits(input));
    }
}
