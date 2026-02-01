using DataFac.Memory;
using DataFac.UnsafeHelpers;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataFac.Storage
{
    public readonly struct BlobIdV1 : IEquatable<BlobIdV1>
    {
        public const int Size = 64;
        //private readonly ReadOnlyMemory<byte> _memory;
        private readonly BlockB064 _block;

        // map
        //  offset  path        len fieldname
        //  00      A.A.A.A.A.A 1   Marker00 '|' for non-embedded or compAlgo char code if embedded
        //  01      A.A.A.A.A.B 1   Marker01 '_' for non-embedded or data length if embedded
        //  02      A.A.A.A.B.A 1   MajorVer
        //  03      A.A.A.A.B.B 1   MinorVer
        //  04      A.A.A.B.A.A 1   CompAlgo
        //  05      A.A.A.B.A.B 1   HashAlgo
        //  06-07   A.A.A.B.B   2   -unused-
        //  08-0B   A.A.B.A     4   BlobSize
        //  0C-0F   A.A.B.B     4   CompSize
        //  10-1F   A.B         16  -unused-
        //  20-3F   B           32  HashData
        public byte Marker00 => _block.A.A.A.A.A.A.ByteValue; // _memory.Span[0];
        public byte Marker01 => _block.A.A.A.A.A.B.ByteValue; // _memory.Span[1];
        public byte MajorVer => _block.A.A.A.A.B.A.ByteValue; // _memory.Span[2];
        public byte MinorVer => _block.A.A.A.A.B.B.ByteValue; // _memory.Span[3];
        public BlobCompAlgo CompAlgo => (BlobCompAlgo)_block.A.A.A.B.A.A.ByteValue; // _memory.Span[4];
        public BlobHashAlgo HashAlgo => (BlobHashAlgo)_block.A.A.A.B.A.B.ByteValue; // _memory.Span[5];
        public int BlobSize => _block.A.A.B.A.Int32ValueLE; // Codec_Int32_LE.ReadFromSpan(_memory.Span.Slice(8, 4));
        public int CompSize => _block.A.A.B.B.Int32ValueLE; // Codec_Int32_LE.ReadFromSpan(_memory.Span.Slice(12, 4));
        public BlockB032 HashData => _block.B; // _memory.Slice(32, 32);

        //public BlockB064 Block => _block;
        public bool IsDefault
        {
            get
            {
                return _block.A.A.A.Int64ValueLE == 0L
                    && _block.A.A.B.Int64ValueLE == 0L
                    && _block.A.B.A.Int64ValueLE == 0L
                    && _block.A.B.B.Int64ValueLE == 0L
                    && _block.B.A.A.Int64ValueLE == 0L
                    && _block.B.A.B.Int64ValueLE == 0L
                    && _block.B.B.A.Int64ValueLE == 0L
                    && _block.B.B.B.Int64ValueLE == 0L
                    ;
                //old
                //ReadOnlySpan<long> nums = MemoryMarshal.Cast<byte, long>(_memory.Span);
                //for (int i = 0; i < nums.Length; i++)
                //{
                //    if (nums[i] != 0) return false;
                //}
                //return true;
            }
        }

        private BlobIdV1(ReadOnlySpan<byte> source)
        {
            if (source.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(source));
            _block.TryRead(source);
        }

        public static BlobIdV1 FromSequence(ReadOnlySequence<byte> source)
        {
            // todo stop allocations?
            return new BlobIdV1(source.Compact().Span);
        }

        public static BlobIdV1 FromSpan(ReadOnlySpan<byte> source)
        {
            return new BlobIdV1(source);
        }

        //public static BlobIdV1 UnsafeWrap(ReadOnlyMemory<byte> memory)
        //{
        //    if (memory.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(memory));
        //    return new BlobIdV1(memory);
        //}

        /// <summary>
        /// Used to directly embed blob data which is small enough into the id.
        /// </summary>
        /// <param name="compAlgo"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException"></exception>
        public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlySequence<byte> data)
        {
            if (data.Length > (BlobIdV1.Size - 2)) throw new ArgumentException("Length must be <= 62", nameof(data));
            _block.A.A.A.A.A.A.ByteValue = compAlgo.ToCharCode();
            _block.A.A.A.A.A.B.ByteValue = (byte)(data.Length+(byte)'A');
            data.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(2));
        }

        /// <summary>
        /// Used to directly embed blob data which is small enough into the id.
        /// </summary>
        /// <param name="compAlgo"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException"></exception>
        public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlySpan<byte> data)
        {
            if (data.Length > (BlobIdV1.Size - 2)) throw new ArgumentException("Length must be <= 62", nameof(data));
            _block.A.A.A.A.A.A.ByteValue = compAlgo.ToCharCode();
            _block.A.A.A.A.A.B.ByteValue = (byte)(data.Length + (byte)'A');
            data.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(2));
        }

        private BlobIdV1(byte majorVer, byte minorVer, int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
        {
            if (hashData.Length != 32) throw new ArgumentException("Length must be == 32", nameof(hashData));
            _block.A.A.A.A.A.A.ByteValue = (byte)'|';   // Marker00
            _block.A.A.A.A.A.B.ByteValue = (byte)'_';   // Marker01
            _block.A.A.A.A.B.A.ByteValue = majorVer;
            _block.A.A.A.A.B.B.ByteValue = minorVer;
            _block.A.A.A.B.A.A.ByteValue = (byte)compAlgo;
            _block.A.A.A.B.A.B.ByteValue = (byte)hashAlgo;
            _block.A.A.B.A.Int32ValueLE = blobSize;
            _block.A.A.B.B.Int32ValueLE = compSize;
            hashData.CopyTo(BlockHelper.AsWritableSpan(ref _block).Slice(32, 32));
        }

        public BlobIdV1(int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
            : this(1, 0, blobSize, compAlgo, compSize, hashAlgo, hashData) { }

        public void WriteTo(Span<byte> target) => _block.TryWrite(target);

        public byte[] ToByteArray() => _block.ToByteArray();

        public bool IsEmbedded => (Marker00 != 0) && Marker00 != (byte)'|';

        public bool TryGetEmbeddedBlob(out ReadOnlySequence<byte> embedded)
        {
            embedded = ReadOnlySequence<byte>.Empty;
            if (!IsEmbedded) return false;
            switch(Marker00.ToCompAlgo())
            {
                case BlobCompAlgo.UnComp:
                    int dataSize = Marker01 - (byte)'A';
                    byte[] embeddedData = new byte[dataSize];
                    _block.WriteTo(2, dataSize, embeddedData);
                    embedded = new ReadOnlySequence<byte>(embeddedData);
                    return true;
                case BlobCompAlgo.Brotli:
                    throw new NotImplementedException("Brotli embedded blobs are not implemented yet.");
                    return true;
                case BlobCompAlgo.Snappy:
                    throw new NotImplementedException("Snappy embedded blobs are not implemented yet.");
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Formats the blob id as a round-trip string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsDefault) return string.Empty;
            StringBuilder result = new StringBuilder();
            if (IsEmbedded)
            {
                char marker = (char)Marker00;
                int dataSize = Marker01 - (byte)'A';
                result.Append(marker);
                result.Append(':');
                result.Append(dataSize);
                result.Append(':');
                result.Append(_block.ToBase64String(2, dataSize));
                return result.ToString();
            }

            result.Append($"V{MajorVer}.{MinorVer}:");
            result.Append(BlobSize);
            result.Append(':');
            result.Append((char)CompAlgo.ToCharCode());
            result.Append(':');
            result.Append(CompSize);
            result.Append(':');
            result.Append((int)HashAlgo);
            result.Append(':');
            if (HashAlgo != BlobHashAlgo.Undefined)
            {
                result.Append(_block.ToBase64String(32, 32));
            }
            return result.ToString();
        }

        /// <summary>
        /// Parses a formatted string to a blob id.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static BlobIdV1 FromString(string source)
        {
            if (string.IsNullOrEmpty(source)) return default;
            var sourceSpan = source.AsSpan();
            int partId = 0;
            int blobSize = default;
            int compAlgoInt = default;
            var compAlgo = BlobCompAlgo.UnComp;
            int compSize = default;
            int hashAlgoInt = default;
            var hashAlgo = BlobHashAlgo.Undefined;
            Span<byte> hashSpan = stackalloc byte[32];
            while (sourceSpan.Length > 0)
            {
                var sourceIndex = sourceSpan.IndexOf(':');
                ReadOnlySpan<char> partSpan = sourceIndex >= 0 ? sourceSpan.Slice(0, sourceIndex) : sourceSpan;
                string part = partSpan.ToString();
                switch (partId)
                {
                    case 0: // header
                        if (part != "V1.0") throw new InvalidDataException($"Invalid version: '{part}'.");
                        break;
                    case 1:
                        if (!int.TryParse(part, out blobSize)) throw new InvalidDataException($"Invalid blobSize: '{part}'.");
                        break;
                    case 2:
                        if (!int.TryParse(part, out compAlgoInt)) throw new InvalidDataException($"Invalid compAlgo: '{part}'.");
                        compAlgo = (BlobCompAlgo)compAlgoInt;
                        break;
                    case 3:
                        if (!int.TryParse(part, out compSize)) throw new InvalidDataException($"Invalid compSize: '{part}'.");
                        break;
                    case 4:
                        if (!int.TryParse(part, out hashAlgoInt)) throw new InvalidDataException($"Invalid hashAlgo: '{part}'.");
                        hashAlgo = (BlobHashAlgo)hashAlgoInt;
                        break;
                    case 5:
#if NET8_0_OR_GREATER
                        if (!Convert.TryFromBase64Chars(partSpan, hashSpan, out int bytesDecoded) || bytesDecoded != 32) throw new InvalidDataException($"Invalid hashData: '{part}'.");
#else
                        byte[] hashBytes = Convert.FromBase64String(part);
                        hashBytes.CopyTo(hashSpan);
#endif
                        break;
                    default:
                        throw new InvalidDataException($"Unexpected format: '{source}'");
                }
                // next
                partId++;
                sourceSpan = sourceIndex >= 0 ? sourceSpan.Slice(sourceIndex + 1) : default;
            }
            return new BlobIdV1(blobSize, compAlgo, compSize, hashAlgo, hashSpan);
        }

        public bool Equals(BlobIdV1 that) => _block.Equals(that._block);
        public override bool Equals(object? obj) => obj is BlobIdV1 other && Equals(other);
        public override int GetHashCode() => _block.GetHashCode();

        public static bool operator ==(BlobIdV1 left, BlobIdV1 right) => left.Equals(right);
        public static bool operator !=(BlobIdV1 left, BlobIdV1 right) => !left.Equals(right);
    }
}
