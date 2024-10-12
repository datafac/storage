#nullable enable

namespace DTOMaker.Gentime
{
    public enum UnaryOperator
    {
        None = 0,
        Plus = 1,
        Minus = 2,
        LogicalNot = 3,
        BitwiseNot = 4,
    }
    public enum BinaryOperator
    {
        None = 0,
        Pow = 1,
        Add = 2,
        Sub = 3,
        Mul = 4,
        Div = 5,
        Mod = 6,
        LSS = 7,
        LEQ = 8,
        GTR = 9,
        GEQ = 10,
        EQU = 11,
        NEQ = 12,
        Assign = 13,
        AND = 14,
        OR = 15,
    }
    public enum TertiaryOperator
    {
        None = 0,
        IfThenElse = 1,
    }
}
