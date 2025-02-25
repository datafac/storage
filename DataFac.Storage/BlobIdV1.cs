using DataFac.Memory;
using DataFac.UnsafeHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataFac.Storage
{
    public enum BlobCompAlgo : byte
    {
        None = 0,
        Brotli = 1,
        GZip = 2,
    }
    public enum BlobHashAlgo : byte
    {
        Undefined = 0,
        Sha256 = 1,
    }
    public readonly struct BlobIdV1 : IEquatable<BlobIdV1>
    {
        private readonly BlockB064 _block;
        private readonly int _hashCode;

        // map
        //  offset  path        len fieldname
        //  00-01   A.A.A.A.A   2   -marker-  "|_"
        //  02      A.A.A.A.B.A 1   MajorVer
        //  03      A.A.A.A.B.B 1   MinorVer
        //  04      A.A.A.B.A.A 1   CompAlgo
        //  05      A.A.A.B.A.B 1   HashAlgo
        //  06-07   A.A.A.B.B   2   -unused-
        //  08-0B   A.A.B.A     4   BlobSize
        //  0C-0F   A.A.B.B     4   CompSize
        //  10-1F   A.B         16  -unused-
        //  20-3F   B           32  HashData
        public byte Marker00 => _block.A.A.A.A.A.A.ByteValue;
        public byte Marker01 => _block.A.A.A.A.A.B.ByteValue;
        public byte MajorVer => _block.A.A.A.A.B.A.ByteValue;
        public byte MinorVer => _block.A.A.A.A.B.B.ByteValue;
        public int BlobSize => _block.A.A.B.A.Int32ValueLE;
        public BlobCompAlgo CompAlgo => (BlobCompAlgo)_block.A.A.A.B.A.A.ByteValue;
        public int CompSize => _block.A.A.B.B.Int32ValueLE;
        public BlobHashAlgo HashAlgo => (BlobHashAlgo)_block.A.A.A.B.A.B.ByteValue;
        public BlockB032 HashData => _block.B;

        public bool IsEmpty => _block.IsEmpty;

        public bool IsEmbedded
        {
            get
            {
                char marker = (char)_block.A.A.A.A.A.A.ByteValue;
                return marker switch
                {
                    'U' => true, // embedded, uncompressed
                    'B' => true,    // embedded, Brotli
                    'G'=> true,     // embedded, GZip
                    _ => false
                };
            }
        }

        /// <summary>
        /// Used to directly embed blob data which is small enough into the id.
        /// </summary>
        /// <param name="compAlgo"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException"></exception>
        public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlySpan<byte> data)
        {
            if (data.Length > 62) throw new ArgumentException("Data length must be <= 62");
            Span<byte> block = BlockHelper.AsWritableSpan<BlockB064>(ref _block);
            block[0] = compAlgo switch
            {
                BlobCompAlgo.Brotli => (byte)'B',
                BlobCompAlgo.GZip => (byte)'G',
                _ => (byte)'U'
            };
            block[1] = (byte)data.Length;
            data.CopyTo(block.Slice(2));
        }

        private BlobIdV1(byte majorVer, byte minorVer, int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, BlockB032 hashData)
        {
            _block.A.A.A.A.A.A.ByteValue = (byte)'|';
            _block.A.A.A.A.A.B.ByteValue = (byte)'_';
            _block.A.A.A.A.B.A.ByteValue = majorVer;
            _block.A.A.A.A.B.B.ByteValue = minorVer;
            _block.A.A.B.A.Int32ValueLE = blobSize;
            _block.A.A.A.B.A.A.ByteValue = (byte)compAlgo;
            _block.A.A.B.B.Int32ValueLE = compSize;
            _block.A.A.A.B.A.B.ByteValue = (byte)hashAlgo;
            _block.B = hashData;
            _hashCode = HashCode.Combine(_block);
        }

        public BlobIdV1(int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, BlockB032 hashData)
            : this(1, 0, blobSize, compAlgo, compSize, hashAlgo, hashData) { }

        public BlobIdV1(in ReadOnlySpan<byte> source)
        {
            if (source.Length != 64) throw new ArgumentException($"Expected length to be 64.", nameof(source));
            if (!_block.TryRead(source)) throw new ArgumentException($"Failed to read from", nameof(source));
            _hashCode = HashCode.Combine(_block);
        }

        public BlobIdV1(in BlobIdV1 source)
        {
            _block = source._block;
            _hashCode = HashCode.Combine(_block);
        }

        public byte[] ToArray()
        {
            Span<byte> target = stackalloc byte[64];
            if (!_block.TryWrite(target)) throw new ArgumentException($"Failed to write to", nameof(target));
            return target.ToArray();
        }

        public void WriteTo(Span<byte> target)
        {
            if (!_block.TryWrite(target)) throw new ArgumentException($"Failed to write to", nameof(target));
        }

        public bool IsAssigned => Marker00 != 0;

        public ReadOnlyMemory<byte> GetEmbeddedBlob()
        {
            char marker = (char)Marker00;
            if (marker == 'U' || marker == 'B' || marker == 'G')
            {
                BlockB064 copy = _block;
                ReadOnlySpan<byte> block = BlockHelper.AsReadOnlySpan<BlockB064>(ref copy);
                int dataSize = block[1];
                return block.Slice(2, dataSize).ToArray();
            }
            else
            {
                throw new InvalidOperationException("Does not contain embedded blob!");
            }
        }

        /// <summary>
        /// Formats the blob id as a round-trip string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(_block.IsEmpty) return string.Empty;
            StringBuilder result = new StringBuilder();
            char marker = (char)Marker00;
            if (marker == 'U' || marker == 'B' || marker == 'G')
            {
                BlockB064 copy = _block;
                ReadOnlySpan<byte> block = BlockHelper.AsReadOnlySpan<BlockB064>(ref copy);
                int dataSize = block[1];
                result.Append(marker);
                result.Append(':');
                result.Append(dataSize);
                result.Append(':');
#if NET8_0_OR_GREATER
                result.Append(Convert.ToBase64String(block.Slice(2, dataSize)));
#else
                result.Append(Convert.ToBase64String(block.Slice(2, dataSize).ToArray()));
#endif
                return result.ToString();
            }

            result.Append($"V{MajorVer}.{MinorVer}:");
            result.Append(BlobSize);
            result.Append(':');
            result.Append((int)CompAlgo);
            result.Append(':');
            result.Append(CompSize);
            result.Append(':');
            result.Append((int)HashAlgo);
            result.Append(':');
            if (HashAlgo != BlobHashAlgo.Undefined)
            {
                var hashData = HashData;
                var hashSpan = BlockHelper.AsReadOnlySpan(ref hashData);
#if NET8_0_OR_GREATER
                result.Append(Convert.ToBase64String(hashSpan));
#else
                result.Append(Convert.ToBase64String(hashSpan.ToArray()));
#endif
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
            var compAlgo = BlobCompAlgo.None;
            int compSize = default;
            int hashAlgoInt = default;
            var hashAlgo = BlobHashAlgo.Undefined;
            BlockB032 hashData = default;
            Span<byte> hashSpan = BlockHelper.AsWritableSpan(ref hashData);
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
            return new BlobIdV1(blobSize, compAlgo, compSize, hashAlgo, hashData);
        }

        public bool Equals(BlobIdV1 that) => that._block.Equals(this._block);
        public override bool Equals(object? obj) => obj is BlobIdV1 other && Equals(other);
        public override int GetHashCode() => _hashCode;
        public static bool operator ==(BlobIdV1 left, BlobIdV1 right) => left.Equals(right);
        public static bool operator !=(BlobIdV1 left, BlobIdV1 right) => !left.Equals(right);
    }
}
