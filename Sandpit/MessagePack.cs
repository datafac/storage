using System.Runtime.CompilerServices;
using System;
using MessagePack;
using DataFac.Runtime;

namespace Sandpit.MessagePack
{
    // ---------- implementations
    [MessagePackObject]
    [Union(Equilateral.EntityKey, typeof(Equilateral))]
    [Union(Rectangle.EntityKey, typeof(Rectangle))]
    [Union(RightTriangle.EntityKey, typeof(RightTriangle))]
    [Union(Square.EntityKey, typeof(Square))]
    public abstract partial class Polygon { }
    public partial class Polygon : EntityBase, IPolygon, IFreezable
    {
        // Derived entities: 6
        // - Equilateral
        // - Quadrilateral
        // - Rectangle
        // - RightTriangle
        // - Square
        // - Triangle

        public new const int EntityKey = 1;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        public Polygon() { }
        public Polygon(IPolygon source, bool frozen = false) : base(source, frozen)
        {
        }


        public bool Equals(Polygon? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Polygon other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
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

    [MessagePackObject]
    [Union(Equilateral.EntityKey, typeof(Equilateral))]
    [Union(RightTriangle.EntityKey, typeof(RightTriangle))]
    public abstract partial class Triangle { }
    public partial class Triangle : Polygon, ITriangle, IFreezable
    {
        // Derived entities: 2
        // - Equilateral
        // - RightTriangle

        public new const int EntityKey = 2;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        public Triangle() { }
        public Triangle(ITriangle source, bool frozen = false) : base(source, frozen)
        {
        }


        public bool Equals(Triangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Triangle other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    public partial class Equilateral : Triangle, IEquilateral, IFreezable
    {
        // Derived entities: 0

        public new const int EntityKey = 3;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        protected override IFreezable OnPartCopy() => new Equilateral(this);

        public Equilateral() { }
        public Equilateral(IEquilateral source, bool frozen = false) : base(source, frozen)
        {
            _Length = source.Length;
        }

        [IgnoreMember]
        private Double _Length = default;
        [Key(1)]
        public Double Length
        {
            get => _Length;
            set => _Length = IfNotFrozen(ref value);
        }


        public bool Equals(Equilateral? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_Length.Equals(other.Length)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Equilateral other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Length);
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    [Union(Rectangle.EntityKey, typeof(Rectangle))]
    [Union(Square.EntityKey, typeof(Square))]
    public abstract partial class Quadrilateral { }
    public partial class Quadrilateral : Polygon, IQuadrilateral, IFreezable
    {
        // Derived entities: 2
        // - Rectangle
        // - Square

        public new const int EntityKey = 5;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        public Quadrilateral() { }
        public Quadrilateral(IQuadrilateral source, bool frozen = false) : base(source, frozen)
        {
        }


        public bool Equals(Quadrilateral? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Quadrilateral other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    public partial class Rectangle : Quadrilateral, IRectangle, IFreezable
    {
        // Derived entities: 0

        public new const int EntityKey = 7;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        protected override IFreezable OnPartCopy() => new Rectangle(this);

        public Rectangle() { }
        public Rectangle(IRectangle source, bool frozen = false) : base(source, frozen)
        {
            _Length = source.Length;
            _Height = source.Height;
        }

        [IgnoreMember]
        private Double _Length = default;
        [Key(1)]
        public Double Length
        {
            get => _Length;
            set => _Length = IfNotFrozen(ref value);
        }

        [IgnoreMember]
        private Double _Height = default;
        [Key(2)]
        public Double Height
        {
            get => _Height;
            set => _Height = IfNotFrozen(ref value);
        }


        public bool Equals(Rectangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_Length.Equals(other.Length)) return false;
            if (!_Height.Equals(other.Height)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Rectangle other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Length);
            result.Add(_Height);
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    public partial class RightTriangle : Triangle, IRightTriangle, IFreezable
    {
        // Derived entities: 0

        public new const int EntityKey = 4;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        protected override IFreezable OnPartCopy() => new RightTriangle(this);

        public RightTriangle() { }
        public RightTriangle(IRightTriangle source, bool frozen = false) : base(source, frozen)
        {
            _Length = source.Length;
            _Height = source.Height;
        }

        [IgnoreMember]
        private Double _Length = default;
        [Key(1)]
        public Double Length
        {
            get => _Length;
            set => _Length = IfNotFrozen(ref value);
        }

        [IgnoreMember]
        private Double _Height = default;
        [Key(2)]
        public Double Height
        {
            get => _Height;
            set => _Height = IfNotFrozen(ref value);
        }


        public bool Equals(RightTriangle? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_Length.Equals(other.Length)) return false;
            if (!_Height.Equals(other.Height)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is RightTriangle other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Length);
            result.Add(_Height);
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    public partial class Square : Quadrilateral, ISquare, IFreezable
    {
        // Derived entities: 0

        public new const int EntityKey = 6;
        protected override void OnFreeze()
        {
            base.OnFreeze();
            // todo freezable members
        }

        protected override IFreezable OnPartCopy() => new Square(this);

        public Square() { }
        public Square(ISquare source, bool frozen = false) : base(source, frozen)
        {
            _Length = source.Length;
        }

        [IgnoreMember]
        private Double _Length = default;
        [Key(1)]
        public Double Length
        {
            get => _Length;
            set => _Length = IfNotFrozen(ref value);
        }


        public bool Equals(Square? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (!_Length.Equals(other.Length)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Square other && Equals(other);
        }

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Length);
            return result.ToHashCode();
        }

        [IgnoreMember]
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;
            if (!IsFrozen) return CalcHashCode();
            _hashCode = CalcHashCode();
            return _hashCode.Value;
        }

    }

    [MessagePackObject]
    [Union(Equilateral.EntityKey, typeof(Equilateral))]
    [Union(Polygon.EntityKey, typeof(Polygon))]
    [Union(Quadrilateral.EntityKey, typeof(Quadrilateral))]
    [Union(Rectangle.EntityKey, typeof(Rectangle))]
    [Union(RightTriangle.EntityKey, typeof(RightTriangle))]
    [Union(Square.EntityKey, typeof(Square))]
    [Union(Triangle.EntityKey, typeof(Triangle))]
    public abstract class EntityBase : IFreezable, IEquatable<EntityBase>
    {
        public static EntityBase CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            int bytesRead;
            return entityKey switch
            {
                Equilateral.EntityKey => MessagePackSerializer.Deserialize<Equilateral>(buffer, out bytesRead),
                Rectangle.EntityKey => MessagePackSerializer.Deserialize<Rectangle>(buffer, out bytesRead),
                RightTriangle.EntityKey => MessagePackSerializer.Deserialize<RightTriangle>(buffer, out bytesRead),
                Square.EntityKey => MessagePackSerializer.Deserialize<Square>(buffer, out bytesRead),
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        public EntityBase() { }
        public EntityBase(object? notUsed, bool frozen)
        {
            _frozen = frozen;
        }
        [IgnoreMember]
        private volatile bool _frozen;
        [IgnoreMember]
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            _frozen = true;
            OnFreeze();
        }
        protected virtual IFreezable OnPartCopy() => throw new NotImplementedException();
        public IFreezable PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot call {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ref T IfNotFrozen<T>(ref T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
            return ref value;
        }

        public bool Equals(EntityBase? other) => true;
        public override bool Equals(object? obj) => obj is EntityBase;
        public override int GetHashCode() => 0;
    }
}