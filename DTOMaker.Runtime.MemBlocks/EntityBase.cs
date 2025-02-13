using DataFac.Memory;
using DataFac.Storage;
using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DTOMaker.Runtime.MemBlocks
{
    public enum BlockSizeCode : byte
    {
        B001 = 0,   // 2^0 = 1
        B002 = 1,   // 2^1 = 2
        B004 = 2,   // 2^2 = 4
        B008 = 3,   // 2^3 = 8
        B016 = 4,   // 2^4 = 16
        B032 = 5,
        B064 = 6,
        B128 = 7,
        B256 = 8,
        B512 = 9,
        K001 = 10,
        K002 = 11,
        K004 = 12,
        K008 = 13,
        //K016 = 14,
        //K032 = 15,
        //etc.
    }

    /// <summary>
    /// todo BlockStructure tests
    /// </summary>
    public readonly struct BlockStructure
    {
        private readonly BlockB064 _header;
        private readonly int _totalBlockLength;

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        //     1     0     1        Byte    LE    HeaderMajorVersion
        //     2     1     1        Byte    LE    HeaderMinorVersion
        //     3     2     1        Byte    LE    HeaderBlockSize
        //     4     3     1        Byte    LE    ClassHeight
        //     5     4     1        Byte    LE    BlockSize1
        //     6     5     1        Byte    LE    BlockSize2
        //     7     6     1        Byte    LE    BlockSize3
        //     8     7     1        Byte    LE    BlockSize4
        //     9     8     1        Byte    LE    BlockSize5
        //    10     9     1        Byte    LE    BlockSize6
        //    11    10     1        Byte    LE    BlockSize7
        //    12    11     1        Byte    LE    BlockSize8
        //    13    12     4        Int32   LE    Spare1
        //    14    16    16        Guid    LE    EntityGuid
        //    15    32    16        Guid    LE    Spare2
        //    16    48    16        Guid    LE    Spare3
        // ------------------------------------------------------------

        public byte HeaderMajorVersion => _header.A.A.A.A.A.A.ByteValue;
        public byte HeaderMinorVersion => _header.A.A.A.A.A.A.ByteValue;
        public ulong Structure => _header.A.A.B.UInt64ValueLE;
        public byte ClassHeight => _header.A.A.B.A.A.A.ByteValue;
        public byte BlockSize01 => _header.A.A.B.A.A.B.ByteValue;
        public byte BlockSize02 => _header.A.A.B.A.B.A.ByteValue;
        public byte BlockSize03 => _header.A.A.B.A.B.B.ByteValue;
        public byte BlockSize04 => _header.A.A.B.B.A.A.ByteValue;
        public byte BlockSize05 => _header.A.A.B.B.A.B.ByteValue;
        public byte BlockSize06 => _header.A.A.B.B.B.A.ByteValue;
        public byte BlockSize07 => _header.A.A.B.B.B.B.ByteValue;
        public int TotalLength => _totalBlockLength;

        private BlockStructure(BlockB064 header, int totalLength)
        {
            _header = header;
            _totalBlockLength = totalLength;
        }

        private BlockStructure(int ch, int b1, int b2, int b3, int b4, int b5, int b6, int b7)
        {
            BlockB064 header = default;
            header.A.A.A.A.A.A.ByteValue = 1;
            header.A.A.A.A.A.B.ByteValue = 0;
            header.A.A.B.A.A.A.ByteValue = (byte)ch;
            header.A.A.B.A.A.B.ByteValue = (byte)GetBlockSizeCode(b1);
            header.A.A.B.A.B.A.ByteValue = (byte)GetBlockSizeCode(b2);
            header.A.A.B.A.B.B.ByteValue = (byte)GetBlockSizeCode(b3);
            header.A.A.B.B.A.A.ByteValue = (byte)GetBlockSizeCode(b4);
            header.A.A.B.B.A.B.ByteValue = (byte)GetBlockSizeCode(b5);
            header.A.A.B.B.B.A.ByteValue = (byte)GetBlockSizeCode(b6);
            header.A.A.B.B.B.B.ByteValue = (byte)GetBlockSizeCode(b7);
            _header = header;
            _totalBlockLength = 64 + b1 + b2 + b3 + b4 + b5 + b6 + b7;
        }

        public static BlockStructure Create(int classHeight, int blockLength)
        {
            BlockB064 header = default;
            var bc = GetBlockSizeCode(CheckBlockLength(blockLength));
            switch (classHeight)
            {
                case 1: header.A.A.B.A.A.B.ByteValue = (byte)bc; break;
                case 2: header.A.A.B.A.B.A.ByteValue = (byte)bc; break;
                case 3: header.A.A.B.A.B.B.ByteValue = (byte)bc; break;
                case 4: header.A.A.B.B.A.A.ByteValue = (byte)bc; break;
                case 5: header.A.A.B.B.A.B.ByteValue = (byte)bc; break;
                case 6: header.A.A.B.B.B.A.ByteValue = (byte)bc; break;
                case 7: header.A.A.B.B.B.B.ByteValue = (byte)bc; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(classHeight), classHeight, null);
            }
            header.A.A.B.A.A.A.ByteValue = (byte)classHeight;
            return new BlockStructure(header, 64 + blockLength);
        }

        public BlockStructure With(int classHeight, int blockLength)
        {
            BlockB064 header = _header;
            var bc = GetBlockSizeCode(CheckBlockLength(blockLength));
            switch (classHeight)
            {
                case 1: header.A.A.B.A.A.B.ByteValue = (byte)bc; break;
                case 2: header.A.A.B.A.B.A.ByteValue = (byte)bc; break;
                case 3: header.A.A.B.A.B.B.ByteValue = (byte)bc; break;
                case 4: header.A.A.B.B.A.A.ByteValue = (byte)bc; break;
                case 5: header.A.A.B.B.A.B.ByteValue = (byte)bc; break;
                case 6: header.A.A.B.B.B.A.ByteValue = (byte)bc; break;
                case 7: header.A.A.B.B.B.B.ByteValue = (byte)bc; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(classHeight), classHeight, null);
            }
            return new BlockStructure(header, TotalLength + blockLength);
        }

        private static int CheckBlockLength(int blockLength)
        {
            // todo? insist blockLength >= 64 (for hardware sympathy)
            return blockLength switch
            {
                //1 => blockLength,
                //2 => blockLength,
                //4 => blockLength,
                //8 => blockLength,
                //16 => blockLength,
                //32 => blockLength,
                64 => blockLength,
                128 => blockLength,
                256 => blockLength,
                512 => blockLength,
                1 * 1024 => blockLength,
                2 * 1024 => blockLength,
                4 * 1024 => blockLength,
                8 * 1024 => blockLength,
                _ => throw new ArgumentOutOfRangeException(nameof(blockLength), blockLength, null)
            };
        }
        private static BlockSizeCode GetBlockSizeCode(int blockLength)
        {
            return blockLength switch
            {
                //1 => BlockSizeCode.B001,
                //2 => BlockSizeCode.B002,
                //4 => BlockSizeCode.B004,
                //8 => BlockSizeCode.B008,
                //16 => BlockSizeCode.B016,
                //32 => BlockSizeCode.B032,
                64 => BlockSizeCode.B064,
                128 => BlockSizeCode.B128,
                256 => BlockSizeCode.B256,
                512 => BlockSizeCode.B512,
                1 * 1024 => BlockSizeCode.K001,
                2 * 1024 => BlockSizeCode.K002,
                4 * 1024 => BlockSizeCode.K004,
                8 * 1024 => BlockSizeCode.K008,
                _ => throw new ArgumentOutOfRangeException(nameof(blockLength), blockLength, null)
            };
        }
        private static readonly ReadOnlyMemory<int> blockSizes = new int[]
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512,
            1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8
        };
        public static int GetBlockSize(BlockSizeCode code) => blockSizes.Span[(int)code];

    }

    public abstract class EntityBase : IHasEntityId, IMemBlocksEntity, IFreezable, IEquatable<EntityBase>
    {
        #region Static Helpers
        public static async ValueTask<T> CreateEmpty<T>(IDataStore dataStore) where T: class, IMemBlocksEntity, IFreezable, new()
        {
            var empty = new T();
            await empty.Pack(dataStore);
            empty.Freeze();
            return empty;
        }
        #endregion

        private const int ClassHeight = 0;
        private const int BlockOffset = 0;
        private const int BlockLength = 64; // V1.0
        private readonly Memory<byte> _writableLocalBlock;
        private readonly ReadOnlyMemory<byte> _readonlyLocalBlock;
        protected readonly Memory<byte> _writableTotalBlock;
        protected readonly ReadOnlyMemory<byte> _readonlyTotalBlock;

        public const string EntityId = "EntityBase";
        protected abstract string OnGetEntityId();
        public string GetEntityId() => OnGetEntityId();

        protected EntityBase(BlockStructure structure)
        {
            _readonlyTotalBlock = _writableTotalBlock = new byte[structure.TotalLength];
            _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
        }
        protected EntityBase(BlockStructure structure, object source)
        {
            if (source is EntityBase concrete)
            {
                // todo copy source buffer if frozen and same structure
                // todo copy slices if source buffer structure not same
                _writableTotalBlock = new byte[structure.TotalLength];
                concrete._readonlyTotalBlock.CopyTo(_writableLocalBlock);
                _readonlyTotalBlock = _writableTotalBlock;
                _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
            }
            else
            {
                _readonlyTotalBlock = _writableTotalBlock = new byte[structure.TotalLength];
                _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
            }
        }
        protected EntityBase(BlockStructure structure, ReadOnlyMemory<byte> buffer)
        {
            // todo copy slices if source buffer structure not same
            _readonlyTotalBlock = buffer;
            _writableTotalBlock = Memory<byte>.Empty;
            _readonlyLocalBlock = _readonlyTotalBlock.Slice(BlockOffset, BlockLength);
            _writableLocalBlock = Memory<byte>.Empty;
            _frozen = true;
        }
        private volatile bool _frozen;
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            if (!_packed) ThrowIsNotPackedException();
            OnFreeze();
            _frozen = true;
        }

        protected abstract int OnGetClassHeight();
        public ReadOnlyMemory<byte> GetBuffer()
        {
            return _frozen ? _readonlyTotalBlock : _readonlyTotalBlock.ToArray();
            // old
            //string entityId = OnGetEntityId();
            //int height = OnGetClassHeight();
            //var buffers = new ReadOnlyMemory<byte>[height];
            //OnGetBuffers(buffers);
            //return DataFac.MemBlocks.Protocol.CombineBuffers(entityId, buffers);
        }

        //public void LoadBuffer(ReadOnlyMemory<byte> buffer)
        //{
        //    ThrowIfFrozen();
        //    var buffers = DataFac.MemBlocks.Protocol.SplitBuffers(buffer);
        //    OnLoadBuffers(buffers);
        //}

        protected virtual IFreezable OnPartCopy() => throw new NotImplementedException();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfFrozen([CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfNotFrozen<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
            return value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when not frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotFrozen([CallerMemberName] string? methodName = null)
        {
            if (!_frozen) ThrowIsNotFrozenException(methodName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotPackedException() => throw new InvalidOperationException($"Cannot freeze before packing.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsNotUnpackedException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} before unpacking.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNotUnpacked([CallerMemberName] string? methodName = null)
        {
            if (_frozen && !_unpacked) ThrowIsNotUnpackedException(methodName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfUnpacked<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen && !_unpacked) ThrowIsNotUnpackedException(methodName);
            return value;
        }

        protected static T IfNotNull<T>(T? value, [CallerMemberName] string? methodName = null) where T : class
        {
            if (value is not null) return value;
            throw new InvalidOperationException($"Cannot call {methodName} when not set.");
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(GetType());
            result.Add(_readonlyTotalBlock.Length);
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyTotalBlock.Span);
#else
            var byteSpan = _readonlyTotalBlock.Span;
            for (int i = 0; i < byteSpan.Length; i++)
            {
                result.Add(byteSpan[i]);
            }
#endif
            return result.ToHashCode();
        }

        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!_frozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

        private volatile bool _packed;
        protected virtual ValueTask OnPack(IDataStore dataStore) => default;
        public async ValueTask Pack(IDataStore dataStore)
        {
            if (_frozen) return;
            if (_packed) return;
            await OnPack(dataStore);
            _packed = true;
            OnFreeze();
            _frozen = true;
            _unpacked = true;
        }

        private volatile bool _unpacked;
        protected virtual ValueTask OnUnpack(IDataStore dataStore, int depth) => default;
        public async ValueTask Unpack(IDataStore dataStore, int depth = 0)
        {
            ThrowIfNotFrozen();
            if (depth < 0) return;
            if (_unpacked) return;
            await OnUnpack(dataStore, depth);
            _unpacked = true;
        }
        public ValueTask UnpackAll(IDataStore dataStore) => Unpack(dataStore, int.MaxValue);
    }
}
