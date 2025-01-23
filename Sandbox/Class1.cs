using DTOMaker.Models;
using DTOMaker.Models.MessagePack;
using DTOMaker.Runtime;
using MessagePack;
using System;

namespace MyOrg.Models
{
    [Entity]
    [EntityKey(1)]
    public interface INode
    {
        [Member(1)] String Key { get; set; }
    }

    [Entity]
    [EntityKey(2)]
    public interface IStringNode : INode
    {
        [Member(1)] String Value { get; set; }
    }

    [Entity]
    [EntityKey(3)]
    public interface INumericNode : INode
    {
    }

    [Entity]
    [EntityKey(4)]
    public interface IInt64Node : INumericNode
    {
        [Member(1)] Int64 Value { get; set; }
    }

    [Entity]
    [EntityKey(5)]
    public interface IDoubleNode : INumericNode
    {
        [Member(1)] Double Value { get; set; }
    }

    [Entity]
    [EntityKey(6)]
    public interface IBooleanNode : INode
    {
        [Member(1)] Boolean Value { get; set; }
    }

    [Entity]
    [EntityKey(10)]
    public interface ITree
    {
        [Member(1)] ITree? Left { get; set; }
        [Member(2)] ITree? Right { get; set; }
        [Member(3)] INode? Node { get; set; }
    }
}
namespace MyOrg.Models.MessagePack
{
    [MessagePackObject]
    public sealed partial class Int64Node { }
    public partial class Int64Node : MyOrg.Models.MessagePack.NumericNode, IInt64Node, IEquatable<Int64Node>
    {
        // Derived entities: 0

        public new const int EntityKey = 4;

        public new static Int64Node CreateFrom(MyOrg.Models.IInt64Node source)
        {
            return source switch
            {
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static Int64Node CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        protected override IFreezable OnPartCopy() => new Int64Node(this);

        public Int64Node() { }
        public Int64Node(IInt64Node source) : base(source)
        {
            _Value = source.Value;
        }

        [IgnoreMember]
        private Int64 _Value = default;
        [Key(201)]
        public Int64 Value
        {
            get => _Value;
            set => _Value = IfNotFrozen(ref value);
        }


        public bool Equals(Int64Node? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Value != other.Value) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is Int64Node other && Equals(other);
        public static bool operator ==(Int64Node? left, Int64Node? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(Int64Node? left, Int64Node? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Value);
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
    [Union(DoubleNode.EntityKey, typeof(DoubleNode))]
    [Union(Int64Node.EntityKey, typeof(Int64Node))]
    public abstract partial class NumericNode { }
    public partial class NumericNode : MyOrg.Models.MessagePack.Node, INumericNode, IEquatable<NumericNode>
    {
        // Derived entities: 2
        // - DoubleNode
        // - Int64Node

        public new const int EntityKey = 3;

        public new static NumericNode CreateFrom(MyOrg.Models.INumericNode source)
        {
            return source switch
            {
                MyOrg.Models.IDoubleNode source2 => new MyOrg.Models.MessagePack.DoubleNode(source2),
                MyOrg.Models.IInt64Node source2 => new MyOrg.Models.MessagePack.Int64Node(source2),
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static NumericNode CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                MyOrg.Models.MessagePack.DoubleNode.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.DoubleNode>(buffer, out var _),
                MyOrg.Models.MessagePack.Int64Node.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.Int64Node>(buffer, out var _),
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public NumericNode() { }
        public NumericNode(INumericNode source) : base(source)
        {
        }


        public bool Equals(NumericNode? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is NumericNode other && Equals(other);
        public static bool operator ==(NumericNode? left, NumericNode? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(NumericNode? left, NumericNode? right) => left is not null ? !left.Equals(right) : (right is not null);

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
    public sealed partial class DoubleNode { }
    public partial class DoubleNode : MyOrg.Models.MessagePack.NumericNode, IDoubleNode, IEquatable<DoubleNode>
    {
        // Derived entities: 0

        public new const int EntityKey = 5;

        public new static DoubleNode CreateFrom(MyOrg.Models.IDoubleNode source)
        {
            return source switch
            {
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static DoubleNode CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        protected override IFreezable OnPartCopy() => new DoubleNode(this);

        public DoubleNode() { }
        public DoubleNode(IDoubleNode source) : base(source)
        {
            _Value = source.Value;
        }

        [IgnoreMember]
        private Double _Value = default;
        [Key(201)]
        public Double Value
        {
            get => _Value;
            set => _Value = IfNotFrozen(ref value);
        }


        public bool Equals(DoubleNode? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Value != other.Value) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is DoubleNode other && Equals(other);
        public static bool operator ==(DoubleNode? left, DoubleNode? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(DoubleNode? left, DoubleNode? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Value);
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
    [Union(BooleanNode.EntityKey, typeof(BooleanNode))]
    [Union(DoubleNode.EntityKey, typeof(DoubleNode))]
    [Union(Int64Node.EntityKey, typeof(Int64Node))]
    [Union(StringNode.EntityKey, typeof(StringNode))]
    public abstract partial class Node { }
    public partial class Node : DTOMaker.Runtime.MessagePack.EntityBase, INode, IEquatable<Node>
    {
        // Derived entities: 5
        // - BooleanNode
        // - DoubleNode
        // - Int64Node
        // - NumericNode (abstract)
        // - StringNode

        public new const int EntityKey = 1;

        public new static Node CreateFrom(MyOrg.Models.INode source)
        {
            return source switch
            {
                MyOrg.Models.IDoubleNode source2 => new MyOrg.Models.MessagePack.DoubleNode(source2),
                MyOrg.Models.IInt64Node source2 => new MyOrg.Models.MessagePack.Int64Node(source2),
                MyOrg.Models.IBooleanNode source2 => new MyOrg.Models.MessagePack.BooleanNode(source2),
                MyOrg.Models.IStringNode source2 => new MyOrg.Models.MessagePack.StringNode(source2),
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static Node CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                MyOrg.Models.MessagePack.BooleanNode.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.BooleanNode>(buffer, out var _),
                MyOrg.Models.MessagePack.DoubleNode.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.DoubleNode>(buffer, out var _),
                MyOrg.Models.MessagePack.Int64Node.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.Int64Node>(buffer, out var _),
                MyOrg.Models.MessagePack.StringNode.EntityKey => MessagePackSerializer.Deserialize<MyOrg.Models.MessagePack.StringNode>(buffer, out var _),
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public Node() { }
        public Node(INode source) : base(source)
        {
            _Key = source.Key;
        }

        [IgnoreMember]
        private String _Key = string.Empty;
        [Key(1)]
        public String Key
        {
            get => _Key;
            set => _Key = IfNotFrozen(ref value);
        }


        public bool Equals(Node? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Key != other.Key) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is Node other && Equals(other);
        public static bool operator ==(Node? left, Node? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(Node? left, Node? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Key);
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
    public sealed partial class BooleanNode { }
    public partial class BooleanNode : MyOrg.Models.MessagePack.Node, IBooleanNode, IEquatable<BooleanNode>
    {
        // Derived entities: 0

        public new const int EntityKey = 6;

        public new static BooleanNode CreateFrom(MyOrg.Models.IBooleanNode source)
        {
            return source switch
            {
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static BooleanNode CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        protected override IFreezable OnPartCopy() => new BooleanNode(this);

        public BooleanNode() { }
        public BooleanNode(IBooleanNode source) : base(source)
        {
            _Value = source.Value;
        }

        [IgnoreMember]
        private Boolean _Value = default;
        [Key(101)]
        public Boolean Value
        {
            get => _Value;
            set => _Value = IfNotFrozen(ref value);
        }


        public bool Equals(BooleanNode? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Value != other.Value) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is BooleanNode other && Equals(other);
        public static bool operator ==(BooleanNode? left, BooleanNode? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(BooleanNode? left, BooleanNode? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Value);
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
    public sealed partial class StringNode { }
    public partial class StringNode : MyOrg.Models.MessagePack.Node, IStringNode, IEquatable<StringNode>
    {
        // Derived entities: 0

        public new const int EntityKey = 2;

        public new static StringNode CreateFrom(MyOrg.Models.IStringNode source)
        {
            return source switch
            {
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static StringNode CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        protected override IFreezable OnPartCopy() => new StringNode(this);

        public StringNode() { }
        public StringNode(IStringNode source) : base(source)
        {
            _Value = source.Value;
        }

        [IgnoreMember]
        private String _Value = string.Empty;
        [Key(101)]
        public String Value
        {
            get => _Value;
            set => _Value = IfNotFrozen(ref value);
        }


        public bool Equals(StringNode? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Value != other.Value) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is StringNode other && Equals(other);
        public static bool operator ==(StringNode? left, StringNode? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(StringNode? left, StringNode? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Value);
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
    public sealed partial class Tree { }
    public partial class Tree : DTOMaker.Runtime.MessagePack.EntityBase, ITree, IEquatable<Tree>
    {
        // Derived entities: 0

        public new const int EntityKey = 10;

        public new static Tree CreateFrom(MyOrg.Models.ITree source)
        {
            return source switch
            {
                _ => throw new ArgumentException($"Unexpected type: {source.GetType().Name}", nameof(source))
            };
        }

        public new static Tree CreateFrom(int entityKey, ReadOnlyMemory<byte> buffer)
        {
            return entityKey switch
            {
                _ => throw new ArgumentOutOfRangeException(nameof(entityKey), entityKey, null)
            };
        }

        protected override string OnGetEntityId() => EntityKey.ToString();

        protected override void OnFreeze()
        {
            base.OnFreeze();
            _Left?.Freeze();
            _Right?.Freeze();
            _Node?.Freeze();
        }

        protected override IFreezable OnPartCopy() => new Tree(this);

        public Tree() { }
        public Tree(ITree source) : base(source)
        {
            _Left = source.Left is null ? null : new MyOrg.Models.MessagePack.Tree(source.Left);
            _Right = source.Right is null ? null : new MyOrg.Models.MessagePack.Tree(source.Right);
            _Node = source.Node is null ? null : MyOrg.Models.MessagePack.Node.CreateFrom(source.Node);
        }

        [IgnoreMember]
        private MyOrg.Models.MessagePack.Tree? _Left;
        [Key(1)]
        public MyOrg.Models.MessagePack.Tree? Left
        {
            get => _Left;
            set => _Left = IfNotFrozen(ref value);
        }
        MyOrg.Models.ITree? ITree.Left
        {
            get => _Left;
            set
            {
                ThrowIfFrozen();
                _Left = value is null ? null : new MyOrg.Models.MessagePack.Tree(value);
            }
        }

        [IgnoreMember]
        private MyOrg.Models.MessagePack.Tree? _Right;
        [Key(2)]
        public MyOrg.Models.MessagePack.Tree? Right
        {
            get => _Right;
            set => _Right = IfNotFrozen(ref value);
        }
        MyOrg.Models.ITree? ITree.Right
        {
            get => _Right;
            set
            {
                ThrowIfFrozen();
                _Right = value is null ? null : new MyOrg.Models.MessagePack.Tree(value);
            }
        }

        [IgnoreMember]
        private MyOrg.Models.MessagePack.Node? _Node;
        [Key(3)]
        public MyOrg.Models.MessagePack.Node? Node
        {
            get => _Node;
            set => _Node = IfNotFrozen(ref value);
        }
        MyOrg.Models.INode? ITree.Node
        {
            get => _Node;
            set
            {
                ThrowIfFrozen();
                _Node = value is null ? null : MyOrg.Models.MessagePack.Node.CreateFrom(value);
            }
        }


        public bool Equals(Tree? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            if (!base.Equals(other)) return false;
            if (_Left != other.Left) return false;
            if (_Right != other.Right) return false;
            if (_Node != other.Node) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is Tree other && Equals(other);
        public static bool operator ==(Tree? left, Tree? right) => left is not null ? left.Equals(right) : (right is null);
        public static bool operator !=(Tree? left, Tree? right) => left is not null ? !left.Equals(right) : (right is not null);

        private int CalcHashCode()
        {
            HashCode result = new HashCode();
            result.Add(base.GetHashCode());
            result.Add(_Left);
            result.Add(_Right);
            result.Add(_Node);
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
}
