using System;
using System.Buffers.Binary;

namespace DTOMaker.Core.Codecs
{
    public sealed class Codec_Double_LE : Codec_Base<double>
    {
        private Codec_Double_LE() { }
        public static Codec_Double_LE Instance { get; } = new Codec_Double_LE();
        public override double OnRead(ReadOnlySpan<byte> source) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(source));
        public override void OnWrite(Span<byte> target, in double input) => BinaryPrimitives.WriteInt64LittleEndian(target, BitConverter.DoubleToInt64Bits(input));
    }
}
