#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace DTOMaker.Gentime
{
    public interface IEntityBase
    {
        bool IsFreezable();

        bool IsFrozen();

        void Freeze();

        bool TryFreeze();
    }
    public abstract class EntityBase : IEntityBase
    {
        public static EntityBase Empty => throw new NotSupportedException();
        public const int EntityTag = 0;
        public EntityBase() { }
        public EntityBase(EntityBase? source) { }
        protected abstract int OnGetEntityTag();
        public int GetEntityTag() => OnGetEntityTag();
        public override int GetHashCode() => 0;

        protected volatile bool _isFrozen = false;
        public bool IsFreezable() => true;
        public bool IsFrozen() => _isFrozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_isFrozen) return;
            OnFreeze();
            _isFrozen = true;
        }
        public bool TryFreeze()
        {
            if (_isFrozen) return false;
            OnFreeze();
            _isFrozen = true;
            return true;
        }
    }


    public partial class Node : EntityBase
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 1;
        protected override int OnGetEntityTag() => EntityTag;


        public Node() : base()
        {
        }

        public Node(Node? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
        }

    }

    public partial class ErrorNode
    {
        private static ErrorNode CreateEmpty()
        {
            var empty = new ErrorNode();
            empty.Freeze();
            return empty;
        }
        private static readonly ErrorNode _empty = CreateEmpty();
        public static new ErrorNode Empty => _empty;

    }
    public partial class ErrorNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 2;
        protected override int OnGetEntityTag() => EntityTag;

        private String? field_Message;
        public String? Message
        {
            get => field_Message;
            set => field_Message = CheckNotFrozen(ref value);
        }

        public ErrorNode() : base()
        {
        }

        public ErrorNode(ErrorNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Message = source.Message;
        }

    }

    public partial class ConstantNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 3;
        protected override int OnGetEntityTag() => EntityTag;


        public ConstantNode() : base()
        {
        }

        public ConstantNode(ConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
        }

    }

    public partial class NullConstantNode
    {
        private static NullConstantNode CreateEmpty()
        {
            var empty = new NullConstantNode();
            empty.Freeze();
            return empty;
        }
        private static readonly NullConstantNode _empty = CreateEmpty();
        public static new NullConstantNode Empty => _empty;

    }
    public partial class NullConstantNode : ConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 4;
        protected override int OnGetEntityTag() => EntityTag;


        public NullConstantNode() : base()
        {
        }

        public NullConstantNode(NullConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
        }

    }

    public partial class BooleanConstantNode
    {
        private static BooleanConstantNode CreateEmpty()
        {
            var empty = new BooleanConstantNode();
            empty.Freeze();
            return empty;
        }
        private static readonly BooleanConstantNode _empty = CreateEmpty();
        public static new BooleanConstantNode Empty => _empty;

    }
    public partial class BooleanConstantNode : ConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 5;
        protected override int OnGetEntityTag() => EntityTag;

        private Boolean field_Value;
        public Boolean Value
        {
            get => field_Value;
            set => field_Value = CheckNotFrozen(ref value);
        }

        public BooleanConstantNode() : base()
        {
        }

        public BooleanConstantNode(BooleanConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Value = source.Value;
        }

    }

    public partial class StringConstantNode
    {
        private static StringConstantNode CreateEmpty()
        {
            var empty = new StringConstantNode();
            empty.Freeze();
            return empty;
        }
        private static readonly StringConstantNode _empty = CreateEmpty();
        public static new StringConstantNode Empty => _empty;

    }
    public partial class StringConstantNode : ConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 6;
        protected override int OnGetEntityTag() => EntityTag;

        private String? field_Value;
        public String? Value
        {
            get => field_Value;
            set => field_Value = CheckNotFrozen(ref value);
        }

        public StringConstantNode() : base()
        {
        }

        public StringConstantNode(StringConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Value = source.Value;
        }

    }

    public partial class NumericConstantNode : ConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 7;
        protected override int OnGetEntityTag() => EntityTag;


        public NumericConstantNode() : base()
        {
        }

        public NumericConstantNode(NumericConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
        }

    }

    public partial class IntegerConstantNode
    {
        private static IntegerConstantNode CreateEmpty()
        {
            var empty = new IntegerConstantNode();
            empty.Freeze();
            return empty;
        }
        private static readonly IntegerConstantNode _empty = CreateEmpty();
        public static new IntegerConstantNode Empty => _empty;

    }
    public partial class IntegerConstantNode : NumericConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 8;
        protected override int OnGetEntityTag() => EntityTag;

        private Int64 field_Value;
        public Int64 Value
        {
            get => field_Value;
            set => field_Value = CheckNotFrozen(ref value);
        }

        public IntegerConstantNode() : base()
        {
        }

        public IntegerConstantNode(IntegerConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Value = source.Value;
        }

    }

    public partial class DoubleConstantNode
    {
        private static DoubleConstantNode CreateEmpty()
        {
            var empty = new DoubleConstantNode();
            empty.Freeze();
            return empty;
        }
        private static readonly DoubleConstantNode _empty = CreateEmpty();
        public static new DoubleConstantNode Empty => _empty;

    }
    public partial class DoubleConstantNode : NumericConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 9;
        protected override int OnGetEntityTag() => EntityTag;

        private Double field_Value;
        public Double Value
        {
            get => field_Value;
            set => field_Value = CheckNotFrozen(ref value);
        }

        public DoubleConstantNode() : base()
        {
        }

        public DoubleConstantNode(DoubleConstantNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Value = source.Value;
        }

    }

    public partial class VariableNode
    {
        private static VariableNode CreateEmpty()
        {
            var empty = new VariableNode();
            empty.Freeze();
            return empty;
        }
        private static readonly VariableNode _empty = CreateEmpty();
        public static new VariableNode Empty => _empty;

    }
    public partial class VariableNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 10;
        protected override int OnGetEntityTag() => EntityTag;

        private String? field_Name;
        public String? Name
        {
            get => field_Name;
            set => field_Name = CheckNotFrozen(ref value);
        }

        public VariableNode() : base()
        {
        }

        public VariableNode(VariableNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Name = source.Name;
        }

    }

    public partial class OperatorNode : ConstantNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 11;
        protected override int OnGetEntityTag() => EntityTag;


        public OperatorNode() : base()
        {
        }

        public OperatorNode(OperatorNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
        }

    }

    public partial class BinaryOperatorNode
    {
        private static BinaryOperatorNode CreateEmpty()
        {
            var empty = new BinaryOperatorNode();
            empty.Freeze();
            return empty;
        }
        private static readonly BinaryOperatorNode _empty = CreateEmpty();
        public static new BinaryOperatorNode Empty => _empty;

    }
    public partial class BinaryOperatorNode : OperatorNode
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            base.OnFreeze();
        }

        public new const int EntityTag = 12;
        protected override int OnGetEntityTag() => EntityTag;

        private BinaryOperator field_Value;
        public BinaryOperator Value
        {
            get => field_Value;
            set => field_Value = CheckNotFrozen(ref value);
        }

        public BinaryOperatorNode() : base()
        {
        }

        public BinaryOperatorNode(BinaryOperatorNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Value = source.Value;
        }

    }

    public partial class UnaryExpressionNode
    {
        private static UnaryExpressionNode CreateEmpty()
        {
            var empty = new UnaryExpressionNode();
            empty.Freeze();
            return empty;
        }
        private static readonly UnaryExpressionNode _empty = CreateEmpty();
        public static new UnaryExpressionNode Empty => _empty;

    }
    public partial class UnaryExpressionNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            field_Operand?.Freeze();
            base.OnFreeze();
        }

        public new const int EntityTag = 13;
        protected override int OnGetEntityTag() => EntityTag;

        private UnaryOperator field_Op;
        public UnaryOperator Op
        {
            get => field_Op;
            set => field_Op = CheckNotFrozen(ref value);
        }
        private Node? field_Operand;
        public Node? Operand
        {
            get => field_Operand;
            set => field_Operand = CheckNotFrozen(ref value);
        }

        public UnaryExpressionNode() : base()
        {
        }

        public UnaryExpressionNode(UnaryExpressionNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Op = source.Op;
            field_Operand = source.Operand;
        }

    }

    public partial class BinaryExpressionNode
    {
        private static BinaryExpressionNode CreateEmpty()
        {
            var empty = new BinaryExpressionNode();
            empty.Freeze();
            return empty;
        }
        private static readonly BinaryExpressionNode _empty = CreateEmpty();
        public static new BinaryExpressionNode Empty => _empty;

    }
    public partial class BinaryExpressionNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            field_Left?.Freeze();
            field_Right?.Freeze();
            base.OnFreeze();
        }

        public new const int EntityTag = 14;
        protected override int OnGetEntityTag() => EntityTag;

        private BinaryOperator field_Op;
        public BinaryOperator Op
        {
            get => field_Op;
            set => field_Op = CheckNotFrozen(ref value);
        }
        private Node? field_Left;
        public Node? Left
        {
            get => field_Left;
            set => field_Left = CheckNotFrozen(ref value);
        }
        private Node? field_Right;
        public Node? Right
        {
            get => field_Right;
            set => field_Right = CheckNotFrozen(ref value);
        }

        public BinaryExpressionNode() : base()
        {
        }

        public BinaryExpressionNode(BinaryExpressionNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Op = source.Op;
            field_Left = source.Left;
            field_Right = source.Right;
        }

    }

    public partial class TertiaryExpressionNode
    {
        private static TertiaryExpressionNode CreateEmpty()
        {
            var empty = new TertiaryExpressionNode();
            empty.Freeze();
            return empty;
        }
        private static readonly TertiaryExpressionNode _empty = CreateEmpty();
        public static new TertiaryExpressionNode Empty => _empty;

    }
    public partial class TertiaryExpressionNode : Node
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowIsReadonly()
        {
            throw new InvalidOperationException("Cannot set properties when frozen");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T CheckNotFrozen<T>(ref T value)
        {
            if (_isFrozen) ThrowIsReadonly();
            return ref value;
        }

        protected override void OnFreeze()
        {
            field_Node1?.Freeze();
            field_Node2?.Freeze();
            field_Node3?.Freeze();
            base.OnFreeze();
        }

        public new const int EntityTag = 15;
        protected override int OnGetEntityTag() => EntityTag;

        private TertiaryOperator field_Op;
        public TertiaryOperator Op
        {
            get => field_Op;
            set => field_Op = CheckNotFrozen(ref value);
        }
        private Node? field_Node1;
        public Node? Node1
        {
            get => field_Node1;
            set => field_Node1 = CheckNotFrozen(ref value);
        }
        private Node? field_Node2;
        public Node? Node2
        {
            get => field_Node2;
            set => field_Node2 = CheckNotFrozen(ref value);
        }
        private Node? field_Node3;
        public Node? Node3
        {
            get => field_Node3;
            set => field_Node3 = CheckNotFrozen(ref value);
        }

        public TertiaryExpressionNode() : base()
        {
        }

        public TertiaryExpressionNode(TertiaryExpressionNode? source) : base(source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            field_Op = source.Op;
            field_Node1 = source.Node1;
            field_Node2 = source.Node2;
            field_Node3 = source.Node3;
        }

    }

}
