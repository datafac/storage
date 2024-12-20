using DataFac.Memory;
using DataFac.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandpit.MemBlocks
{
    public abstract class EntityBase : IFreezable, IEquatable<EntityBase>
    {
        public static EntityBase CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                Equilateral.EntityId => new Equilateral(buffers),
                Polygon.EntityId => new Polygon(buffers),
                Quadrilateral.EntityId => new Quadrilateral(buffers),
                Rectangle.EntityId => new Rectangle(buffers),
                RightTriangle.EntityId => new RightTriangle(buffers),
                Square.EntityId => new Square(buffers),
                Triangle.EntityId => new Triangle(buffers),
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        public EntityBase() { }
        public EntityBase(object? notUsed, bool frozen)
        {
            _frozen = frozen;
        }
        public EntityBase(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            _frozen = true;
        }
        private volatile bool _frozen = false;
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            _frozen = true;
            OnFreeze();
        }

        protected abstract string OnGetEntityId();
        public string GetEntityId() => OnGetEntityId();
        protected abstract int OnGetClassHeight();
        protected virtual void OnGetBuffers(ReadOnlyMemory<byte>[] buffers) { }
        public ReadOnlyMemory<ReadOnlyMemory<byte>> GetBuffers()
        {
            int height = OnGetClassHeight();
            ReadOnlyMemory<byte>[] buffers = new ReadOnlyMemory<byte>[height];
            OnGetBuffers(buffers);
            return buffers;
        }
        protected virtual IFreezable OnPartCopy() => throw new NotImplementedException();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowExceptionIfFrozen([CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => 0;
    }

    public partial class Polygon : EntityBase, IPolygon, IEquatable<Polygon>
    {
        // Derived entities: 6
        // - Equilateral
        // - Quadrilateral
        // - Rectangle
        // - RightTriangle
        // - Square
        // - Triangle

        private const int ClassHeight = 1;
        private const int BlockLength = 0;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Polygon";

        public new static Polygon CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                Equilateral.EntityId => new Equilateral(buffers),
                Quadrilateral.EntityId => new Quadrilateral(buffers),
                Rectangle.EntityId => new Rectangle(buffers),
                RightTriangle.EntityId => new RightTriangle(buffers),
                Square.EntityId => new Square(buffers),
                Triangle.EntityId => new Triangle(buffers),
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        // ------------------------------------------------------------

        public Polygon()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Polygon(Polygon source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Polygon(IPolygon source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Polygon(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }


        public bool Equals(Polygon? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Polygon other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class Triangle : Polygon, ITriangle, IEquatable<Triangle>
    {
        // Derived entities: 2
        // - Equilateral
        // - RightTriangle

        private const int ClassHeight = 2;
        private const int BlockLength = 0;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Triangle";

        public new static Triangle CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                Equilateral.EntityId => new Equilateral(buffers),
                RightTriangle.EntityId => new RightTriangle(buffers),
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        // ------------------------------------------------------------

        public Triangle()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Triangle(Triangle source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Triangle(ITriangle source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Triangle(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }


        public bool Equals(Triangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Triangle other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class Quadrilateral : Polygon, IQuadrilateral, IEquatable<Quadrilateral>
    {
        // Derived entities: 2
        // - Rectangle
        // - Square

        private const int ClassHeight = 2;
        private const int BlockLength = 0;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Quadrilateral";

        public new static Quadrilateral CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                Rectangle.EntityId => new Rectangle(buffers),
                Square.EntityId => new Square(buffers),
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        // ------------------------------------------------------------

        public Quadrilateral()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Quadrilateral(Quadrilateral source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Quadrilateral(IQuadrilateral source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Quadrilateral(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }


        public bool Equals(Quadrilateral? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Quadrilateral other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class Equilateral : Triangle, IEquilateral, IEquatable<Equilateral>
    {
        // Derived entities: 0

        private const int ClassHeight = 3;
        private const int BlockLength = 8;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Equilateral";

        public new static Equilateral CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        //     1     0     8        Double  LE    Length
        // ------------------------------------------------------------

        public Equilateral()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Equilateral(Equilateral source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Equilateral(IEquilateral source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
            this.Length = source.Length;
        }

        public Equilateral(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }

        public Double Length
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(0, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(0, 8).Span, value);
            }
        }


        public bool Equals(Equilateral? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Equilateral other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class RightTriangle : Triangle, IRightTriangle, IEquatable<RightTriangle>
    {
        // Derived entities: 0

        private const int ClassHeight = 3;
        private const int BlockLength = 16;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "RightTriangle";

        public new static RightTriangle CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        //     1     0     8        Double  LE    Length
        //     2     8     8        Double  LE    Height
        // ------------------------------------------------------------

        public RightTriangle()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public RightTriangle(RightTriangle source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public RightTriangle(IRightTriangle source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
            this.Length = source.Length;
            this.Height = source.Height;
        }

        public RightTriangle(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }

        public Double Length
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(0, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(0, 8).Span, value);
            }
        }

        public Double Height
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(8, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(8, 8).Span, value);
            }
        }


        public bool Equals(RightTriangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is RightTriangle other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class Rectangle : Quadrilateral, IRectangle, IEquatable<Rectangle>
    {
        // Derived entities: 0

        private const int ClassHeight = 3;
        private const int BlockLength = 16;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Rectangle";

        public new static Rectangle CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        //     1     0     8        Double  LE    Length
        //     2     8     8        Double  LE    Height
        // ------------------------------------------------------------

        public Rectangle()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Rectangle(Rectangle source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Rectangle(IRectangle source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
            this.Length = source.Length;
            this.Height = source.Height;
        }

        public Rectangle(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }

        public Double Length
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(0, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(0, 8).Span, value);
            }
        }

        public Double Height
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(8, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(8, 8).Span, value);
            }
        }


        public bool Equals(Rectangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Rectangle other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    public partial class Square : Quadrilateral, ISquare, IEquatable<Square>
    {
        // Derived entities: 0

        private const int ClassHeight = 3;
        private const int BlockLength = 8;
        private readonly Memory<byte> _writableBlock;
        private readonly ReadOnlyMemory<byte> _readonlyBlock;

        public new const string EntityId = "Square";

        public new static Square CreateFrom(string entityId, ReadOnlyMemory<ReadOnlyMemory<byte>> buffers)
        {
            return entityId switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityId), entityId, null)
            };
        }

        protected override string OnGetEntityId() => EntityId;
        protected override int OnGetClassHeight() => ClassHeight;
        protected override void OnGetBuffers(ReadOnlyMemory<byte>[] buffers)
        {
            base.OnGetBuffers(buffers);
            var block = IsFrozen ? _readonlyBlock : _writableBlock.ToArray();
            buffers[ClassHeight - 1] = block;
        }

        // -------------------- field map -----------------------------
        //  Seq.  Off.  Len.  N.    Type    End.  Name
        //  ----  ----  ----  ----  ------- ----  -------
        //     1     0     8        Double  LE    Length
        // ------------------------------------------------------------

        public Square()
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
        }

        public Square(Square source, bool frozen = false) : base(source, frozen)
        {
            _writableBlock = source._writableBlock.ToArray();
            _readonlyBlock = _writableBlock;
        }

        public Square(ISquare source, bool frozen = false) : base(source, frozen)
        {
            _readonlyBlock = _writableBlock = new byte[BlockLength];
            this.Length = source.Length;
        }

        public Square(ReadOnlyMemory<ReadOnlyMemory<byte>> buffers) : base(buffers)
        {
            ReadOnlyMemory<byte> source = buffers.Span[ClassHeight - 1];
            if (source.Length >= BlockLength)
            {
                _readonlyBlock = source.Slice(0, BlockLength);
            }
            else
            {
                // forced copy as source is too short
                Memory<byte> memory = new byte[BlockLength];
                source.Span.CopyTo(memory.Span);
                _readonlyBlock = memory;
            }
            _writableBlock = Memory<byte>.Empty;
        }

        public Double Length
        {
            get
            {
                return (Double)Codec_Double_LE.ReadFromSpan(_readonlyBlock.Slice(0, 8).Span);
            }

            set
            {
                ThrowExceptionIfFrozen();
                Codec_Double_LE.WriteToSpan(_writableBlock.Slice(0, 8).Span, value);
            }
        }


        public bool Equals(Square? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_readonlyBlock.Span.SequenceEqual(other._readonlyBlock.Span)) return false;
            return true;
        }
        public override bool Equals(object? obj) => obj is Square other && Equals(other);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
#if NET8_0_OR_GREATER
            result.AddBytes(_readonlyBlock.Span);
#else
            var byteSpan = _readonlyBlock.Span;
            result.Add(byteSpan.Length);
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
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }
}
