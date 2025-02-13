using DataFac.Memory;
using DataFac.Storage;
using System;
using System.Buffers;
using System.IO;
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
        /// <summary>
        /// Marker and version bytes
        /// </summary>
        public readonly long SignatureCode;

        /// <summary>
        /// Class height and block size codes
        /// </summary>
        public readonly long StructureCode;

        /// <summary>
        /// Entity identifier
        /// </summary>
        public readonly Guid EntityGuid;

        public readonly int EffectiveLength;

        public BlockStructure(long signature, long structure, Guid entityGuid)
        {
            SignatureCode = signature;
            StructureCode = structure;
            EntityGuid = entityGuid;
            EffectiveLength = GetEffectiveLength(structure);
        }

        public BlockStructure(ReadOnlySpan<byte> source)
        {
            SignatureCode = Codec_Int64_LE.ReadFromSpan(source.Slice(0, 8));
            StructureCode = Codec_Int64_LE.ReadFromSpan(source.Slice(8, 8));
            EntityGuid = Codec_Guid_LE.ReadFromSpan(source.Slice(16, 16));
            EffectiveLength = GetEffectiveLength(StructureCode);
        }

        public void WriteTo(Span<byte> target)
        {
            Codec_Int64_LE.WriteToSpan(target.Slice(0, 8), SignatureCode);
            Codec_Int64_LE.WriteToSpan(target.Slice(8, 8), StructureCode);
            Codec_Guid_LE.WriteToSpan(target.Slice(16, 16), EntityGuid);
        }

        private static readonly int[] _effectiveBlockSizes = [64, 64, 64, 64, 64, 64, 64, 128, 256, 512, 1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8, 1024 * 16, 1024 * 32];
        private static int GetEffectiveBlockSize(int code)
        {
            if (code < 0 || code >= 16) throw new ArgumentOutOfRangeException(nameof(code), code, null);
            return _effectiveBlockSizes[code];
        }

        private static int GetEffectiveLength(long structureCode)
        {
            int classHeight = (int)(structureCode & 0x0F);
            int totalLength = 64;
            long bits = structureCode;
            for (int h = 0; h < classHeight && h < 15; h++)
            {
                bits = bits >> 4;
                int blockLength = GetEffectiveBlockSize((int)(bits & 0x0F));
                totalLength += blockLength;
            }
            return totalLength;
        }

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

        protected EntityBase(BlockStructure thisStructure)
        {
            _readonlyTotalBlock = _writableTotalBlock = new byte[thisStructure.EffectiveLength];
            _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
            thisStructure.WriteTo(_writableLocalBlock.Span);
        }
        protected static void CheckStructures(BlockStructure thisStructure, BlockStructure thatStructure)
        {
            if (thatStructure.SignatureCode != thisStructure.SignatureCode)
            {
                // todo support minor version change
                throw new NotSupportedException($"Cannot read source with unknown signature ({thatStructure.SignatureCode}), expected ({thisStructure.SignatureCode}).");
            }
            if (thatStructure.EntityGuid != thisStructure.EntityGuid)
            {
                // type mismatch
                throw new InvalidDataException($"Cannot read source with unknown entity id ({thatStructure.EntityGuid}), expected ({thisStructure.EntityGuid}).");
            }
            if (thatStructure.StructureCode != thisStructure.StructureCode)
            {
                // todo structure conversion
                throw new NotSupportedException($"Cannot read source with different structure ({thatStructure.StructureCode}), expected ({thisStructure.StructureCode}).");
            }
        }
        protected EntityBase(BlockStructure thisStructure, object source)
        {
            if (source is EntityBase sourceEntity)
            {
                _readonlyTotalBlock = _writableTotalBlock = new byte[thisStructure.EffectiveLength];
                _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
                thisStructure.WriteTo(_writableLocalBlock.Span);
                BlockStructure thatStructure = new BlockStructure(sourceEntity._readonlyTotalBlock.Span);
                CheckStructures(thisStructure, thatStructure);
                sourceEntity._readonlyTotalBlock.CopyTo(_writableTotalBlock);
                // todo copy source buffer if frozen and same structure (unsafe?)
                // todo copy slices if source buffer structure not same
            }
            else
            {
                _readonlyTotalBlock = _writableTotalBlock = new byte[thisStructure.EffectiveLength];
                _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
                thisStructure.WriteTo(_writableLocalBlock.Span);
            }
        }
        protected EntityBase(BlockStructure thisStructure, ReadOnlyMemory<byte> buffer)
        {
            BlockStructure thatStructure = new BlockStructure(buffer.Span);
            CheckStructures(thisStructure, thatStructure);
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
