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

        private const long BlockSignatureCode = 89980L; // V1.0
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

        protected static ReadOnlyMemory<byte> CreateHeader(long structureBits, Guid entityGuid)
        {
            Span<byte> header = stackalloc byte[64];
            Codec_Int64_LE.WriteToSpan(header.Slice(0, 8), BlockSignatureCode);
            Codec_Int64_LE.WriteToSpan(header.Slice(8, 8), structureBits);
            Codec_Guid_LE.WriteToSpan(header.Slice(16, 16), entityGuid);
            return header.ToArray();
        }

        private static readonly int[] _blockSizes = [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 * 1, 1024 * 2, 1024 * 4, 1024 * 8, 1024 * 16, 1024 * 32];
        private static int GetEffectiveBlockSize(int code)
        {
            ReadOnlySpan<int> blockSizes = _blockSizes;
            return Math.Max(64, blockSizes[code]);
        }

        protected static int CalculateTotalLength(long structureBits)
        {
            int classHeight = (int)(structureBits & 0x0F);
            int totalLength = 64;
            long bits = structureBits;
            for (int h = 0; h < classHeight && h < 15; h++)
            {
                bits = bits >> 4;
                int blockLength = GetEffectiveBlockSize((int)(bits & 0x0F));
                totalLength += blockLength;
            }
            return totalLength;
        }

        protected EntityBase(BlockHeader blockHeader)
        {
            _readonlyTotalBlock = _writableTotalBlock = new byte[blockHeader.TotalLength];
            _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
            blockHeader.Header.CopyTo(_writableLocalBlock);
        }
        protected static void CheckStructuresqqq(BlockStructureOld thisStructure, BlockStructureOld thatStructure)
        {
            if (thatStructure.SignatureBits != thisStructure.SignatureBits)
            {
                // todo support minor version change
                throw new NotSupportedException($"Cannot read source with unknown signature ({thatStructure.SignatureBits}), expected ({thisStructure.SignatureBits}).");
            }
            if (thatStructure.EntityGuid != thisStructure.EntityGuid)
            {
                // type mismatch
                throw new InvalidDataException($"Cannot read source with unknown entity id ({thatStructure.EntityGuid}), expected ({thisStructure.EntityGuid}).");
            }
            if (thatStructure.StructureBits != thisStructure.StructureBits)
            {
                // todo structure conversion
                throw new NotSupportedException($"Cannot read source with different structure ({thatStructure.StructureBits}), expected ({thisStructure.StructureBits}).");
            }
        }
        protected EntityBase(BlockHeader blockHeader, object source)
        {
            // todo split this method into 2: 1 for concrete, 1 for interface
            _readonlyTotalBlock = _writableTotalBlock = new byte[blockHeader.TotalLength];
            _readonlyLocalBlock = _writableLocalBlock = _writableTotalBlock.Slice(BlockOffset, BlockLength);
            if (source is EntityBase sourceEntity)
            {
                // special case
                ReadOnlySpan<byte> sourceHeader = sourceEntity._readonlyTotalBlock.Slice(BlockOffset, BlockLength).Span;
                if (sourceHeader.SequenceEqual(blockHeader.Header.Span))
                {
                    // identical type and structure - copy everything
                    sourceEntity._readonlyTotalBlock.CopyTo(_writableTotalBlock);
                }
                else
                {
                    // todo check source signature bits
                    // todo throw error if entity guids not equal
                    // todo copy slices if structure not same
                    throw new NotImplementedException();
                }
            }
            else
            {
                blockHeader.Header.CopyTo(_writableLocalBlock);
            }
        }
        protected EntityBase(BlockHeader blockHeader, ReadOnlyMemory<byte> buffer)
        {
            ReadOnlySpan<byte> sourceHeader = buffer.Slice(BlockOffset, BlockLength).Span;
            if (sourceHeader.SequenceEqual(blockHeader.Header.Span))
            {
                // identical type and structure - copy everything
                _readonlyTotalBlock = buffer;
                _writableTotalBlock = Memory<byte>.Empty;
            }
            else
            {
                // todo check source signature bits
                // todo throw error if entity guids not equal
                // todo copy slices if structure not same
                throw new NotImplementedException();
            }
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
