using DataFac.Memory;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DataFac.Storage
{
    public readonly struct BlobIdV1 : IEquatable<BlobIdV1>
    {
        public const int Size = 64;
        private readonly ReadOnlyMemory<byte> _memory;

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
        public byte Marker00 => _memory.Span[0];
        public byte Marker01 => _memory.Span[1];
        public byte MajorVer => _memory.Span[2];
        public byte MinorVer => _memory.Span[3];
        public BlobCompAlgo CompAlgo => (BlobCompAlgo)_memory.Span[4];
        public BlobHashAlgo HashAlgo => (BlobHashAlgo)_memory.Span[5];
        public int BlobSize => Codec_Int32_LE.ReadFromSpan(_memory.Span.Slice(8, 4));
        public int CompSize => Codec_Int32_LE.ReadFromSpan(_memory.Span.Slice(12, 4));
        public ReadOnlyMemory<byte> HashData => _memory.Slice(32, 32);

        public ReadOnlyMemory<byte> Memory => _memory;
        public bool IsDefaultOrAllZero
        {
            get
            {
                ReadOnlySpan<long> nums = MemoryMarshal.Cast<byte, long>(_memory.Span);
                for (int i = 0; i < nums.Length; i++)
                {
                    if (nums[i] != 0) return false;
                }
                return true;
            }
        }

        public bool IsEmbedded
        {
            get
            {
                char marker = (char)Marker00;
                return marker switch
                {
                    'U' => true, // embedded, uncompressed
                    'B' => true,    // embedded, Brotli
                    'G'=> true,     // embedded, GZip
                    _ => false
                };
            }
        }

        private BlobIdV1(ReadOnlyMemory<byte> memory)
        {
            _memory = memory;
        }

        public static BlobIdV1 FromSpan(ReadOnlySpan<byte> source)
        {
            if (source.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(source));
            return new BlobIdV1(source.ToArray());
        }

        public static BlobIdV1 UnsafeWrap(ReadOnlyMemory<byte> memory)
        {
            if (memory.Length != BlobIdV1.Size) throw new ArgumentException($"Length must be {BlobIdV1.Size}.", nameof(memory));
            return new BlobIdV1(memory);
        }

        /// <summary>
        /// Used to directly embed blob data which is small enough into the id.
        /// </summary>
        /// <param name="compAlgo"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException"></exception>
        public BlobIdV1(BlobCompAlgo compAlgo, ReadOnlySpan<byte> data)
        {
            if (data.Length > 62) throw new ArgumentException("Length must be <= 62", nameof(data));
            Memory<byte> memory = new byte[BlobIdV1.Size];
            Span<byte> block = memory.Span;
            block[0] = compAlgo switch
            {
                BlobCompAlgo.Brotli => (byte)'B',
                BlobCompAlgo.GZip => (byte)'G',
                _ => (byte)'U'
            };
            block[1] = (byte)data.Length;
            data.CopyTo(block.Slice(2));
            _memory = memory;
        }

        private BlobIdV1(byte majorVer, byte minorVer, int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
        {
            if (hashData.Length != 32) throw new ArgumentException("Length must be == 32", nameof(hashData));
            Memory<byte> memory = new byte[BlobIdV1.Size];
            Span<byte> block = memory.Span;
            block[0] = (byte)'|';   // Marker00
            block[1] = (byte)'_';   // Marker01
            block[2] = majorVer;
            block[3] = minorVer;
            block[4] = (byte)compAlgo;
            block[5] = (byte)hashAlgo;
            Codec_Int32_LE.WriteToSpan(block.Slice(8, 4), blobSize);
            Codec_Int32_LE.WriteToSpan(block.Slice(12, 4), compSize);
            hashData.CopyTo(block.Slice(32, 32));
            _memory = memory;
        }

        public BlobIdV1(int blobSize, BlobCompAlgo compAlgo, int compSize, BlobHashAlgo hashAlgo, ReadOnlySpan<byte> hashData)
            : this(1, 0, blobSize, compAlgo, compSize, hashAlgo, hashData) { }

        public void WriteTo(Span<byte> target)
        {
            _memory.Span.CopyTo(target);
        }

        public bool IsAssigned => Marker00 != 0;

        public ReadOnlyMemory<byte> GetEmbeddedBlob()
        {
            char marker = (char)Marker00;
            if (marker == 'U' || marker == 'B' || marker == 'G')
            {
                int dataSize = _memory.Span[1];
                return _memory.Slice(2, dataSize);
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
            if (IsDefaultOrAllZero) return string.Empty;
            StringBuilder result = new StringBuilder();
            char marker = (char)Marker00;
            if (marker == 'U' || marker == 'B' || marker == 'G')
            {
                int dataSize = _memory.Span[1];
                result.Append(marker);
                result.Append(':');
                result.Append(dataSize);
                result.Append(':');
#if NET8_0_OR_GREATER
                result.Append(Convert.ToBase64String(_memory.Span.Slice(2, dataSize)));
#else
                result.Append(Convert.ToBase64String(_memory.Slice(2, dataSize).ToArray()));
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
                var hashSpan = HashData.Span;
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

        public bool Equals(BlobIdV1 that) => that._memory.Span.SequenceEqual(this._memory.Span);
        public override bool Equals(object? obj) => obj is BlobIdV1 other && Equals(other);
        public override int GetHashCode()
        {
            var hc = new HashCode();
            ReadOnlySpan<byte> block = _memory.Span;
#if NET8_0_OR_GREATER
            hc.AddBytes(block);
#else
            for (int i = 0; i < block.Length; i++)
            {
                hc.Add(block[i]);
            }
#endif
            return hc.ToHashCode();
        }

        public static bool operator ==(BlobIdV1 left, BlobIdV1 right) => left.Equals(right);
        public static bool operator !=(BlobIdV1 left, BlobIdV1 right) => !left.Equals(right);
    }
}
