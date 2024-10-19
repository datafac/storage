using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public static class NodeHelpers
    {
        internal static BinaryOperator ToBinaryOperator(this Node node)
        {
            return node switch
            {
                SymbolConstantNode sn => sn.Value.Kind switch
                {
                    ExprToken.Plus => BinaryOperator.Add,
                    ExprToken.Dash => BinaryOperator.Sub,
                    ExprToken.Star => BinaryOperator.Mul,
                    ExprToken.Slash => BinaryOperator.Div,
                    ExprToken.Percent => BinaryOperator.Mod,
                    ExprToken.Power => BinaryOperator.Pow,
                    ExprToken.LAngle => BinaryOperator.LSS,
                    ExprToken.LEQ => BinaryOperator.LEQ,
                    ExprToken.RAngle => BinaryOperator.GTR,
                    ExprToken.GEQ => BinaryOperator.GEQ,
                    ExprToken.EQU => BinaryOperator.EQU,
                    ExprToken.NEQ => BinaryOperator.NEQ,
                    ExprToken.AND => BinaryOperator.AND,
                    ExprToken.OR => BinaryOperator.OR,
                    _ => throw new ArgumentOutOfRangeException(nameof(sn.Value.Kind), sn.Value.Kind, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(node), node, null)
            };
        }

        private static object BooleanBinaryOp(BinaryOperator op, bool a, bool b)
        {
            switch (op)
            {
                case BinaryOperator.AND: return a && b;
                case BinaryOperator.OR: return a || b;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static object Int64BinaryOp(BinaryOperator op, long a, long b)
        {
            switch (op)
            {
                case BinaryOperator.Add: return a + b;
                case BinaryOperator.Sub: return a - b;
                case BinaryOperator.Mul: return a * b;
                case BinaryOperator.Div: return a / b;
                case BinaryOperator.Mod: return a % b;
                case BinaryOperator.Pow: return Math.Pow(a, b);
                case BinaryOperator.LSS: return a < b;
                case BinaryOperator.LEQ: return a <= b;
                case BinaryOperator.GTR: return a > b;
                case BinaryOperator.GEQ: return a >= b;
                case BinaryOperator.EQU: return a == b;
                case BinaryOperator.NEQ: return a != b;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static object DoubleBinaryOp(BinaryOperator op, double a, double b)
        {
            switch (op)
            {
                case BinaryOperator.Add: return a + b;
                case BinaryOperator.Sub: return a - b;
                case BinaryOperator.Mul: return a * b;
                case BinaryOperator.Div: return a / b;
                case BinaryOperator.Mod: return a % b;
                case BinaryOperator.Pow: return Math.Pow(a, b);
                case BinaryOperator.LSS: return a < b;
                case BinaryOperator.LEQ: return a <= b;
                case BinaryOperator.GTR: return a > b;
                case BinaryOperator.GEQ: return a >= b;
                case BinaryOperator.EQU: return a == b;
                case BinaryOperator.NEQ: return a != b;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static object StringBinaryOp(BinaryOperator op, string? a, string? b)
        {
            switch (op)
            {
                case BinaryOperator.Add: return a + b;
                case BinaryOperator.LSS: return string.CompareOrdinal(a, b) < 0;
                case BinaryOperator.LEQ: return string.CompareOrdinal(a, b) <= 0;
                case BinaryOperator.GTR: return string.CompareOrdinal(a, b) > 0;
                case BinaryOperator.GEQ: return string.CompareOrdinal(a, b) >= 0;
                case BinaryOperator.EQU: return a == b;
                case BinaryOperator.NEQ: return a != b;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static object EvaluateUntypedBinaryExpression(BinaryOperator op, object? left, object? right)
        {
            return left switch
            {
                null => right switch
                {
                    null => StringBinaryOp(op, null, null),
                    string b => StringBinaryOp(op, null, b),
                    _ => throw new ArgumentOutOfRangeException(nameof(right), right, null)
                },
                string a => right switch
                {
                    null => StringBinaryOp(op, a, null),
                    string s => StringBinaryOp(op, a, s),
                    _ => StringBinaryOp(op, a, right.ToString())
                },
                bool a => right switch
                {
                    bool b => BooleanBinaryOp(op, a, b),
                    _ => throw new ArgumentOutOfRangeException(nameof(right), right, null)
                },
                long a => right switch
                {
                    long b => Int64BinaryOp(op, a, b),
                    double b => DoubleBinaryOp(op, a, b),
                    string s => StringBinaryOp(op, a.ToString(), s),
                    _ => throw new ArgumentOutOfRangeException(nameof(right), right, null)
                },
                double a => right switch
                {
                    long b => DoubleBinaryOp(op, a, b),
                    double b => DoubleBinaryOp(op, a, b),
                    string s => StringBinaryOp(op, a.ToString(), s),
                    _ => throw new ArgumentOutOfRangeException(nameof(right), right, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(left), left, null)
            };
        }

        private static object EvaluateUntypedTertiaryExpression(TertiaryOperator op, object? n1, object? n2, object? n3)
        {
            throw new NotImplementedException();
        }

        private static object? Evaluate(this BinaryExpressionNode node, IDictionary<string, object?> variables)
        {
            // special cases
            if (node.Op == BinaryOperator.Assign)
            {
                if (node.Left is VariableNode vn && vn.Name is not null)
                {
                    var value = node.Right.Evaluate(variables);
                    variables[vn.Name] = value;
                    return value;
                }
                else
                    throw new InvalidCastException($"LHS of '{node}' is not a variable name");
            }
            else
            {
                var left = node.Left.Evaluate(variables);
                var right = node.Right.Evaluate(variables);
                var op = node.Op;
                return EvaluateUntypedBinaryExpression(op, left, right);
            }
        }

        private static object? Evaluate(this VariableNode node, IDictionary<string, object?> variables)
        {
            if (node.Name is null) return null;
            return variables.TryGetValue(node.Name, out var value) ? value : null;
        }

        private static object? Evaluate(this TertiaryExpressionNode node, IDictionary<string, object?> variables)
        {
            // special cases
            if (node.Op == TertiaryOperator.IfThenElse)
            {
                var ifExpr = node.Node1.Evaluate(variables);
                if (ifExpr is bool condition)
                {
                    return condition
                        ? node.Node2.Evaluate(variables)
                        : node.Node3.Evaluate(variables);
                }
                else
                    throw new InvalidCastException($"Cannot cast {ifExpr} to bool");
            }
            else
            {
                var n1 = node.Node1.Evaluate(variables);
                var n2 = node.Node2.Evaluate(variables);
                var n3 = node.Node3.Evaluate(variables);
                var op = node.Op;
                return EvaluateUntypedTertiaryExpression(op, n1, n2, n3);
            }
        }

        public static object? Evaluate(this Node? node, IDictionary<string, object?> variables)
        {
            return node switch
            {
                null => null,
                NullConstantNode => null,
                IntegerConstantNode ic => ic.Value,
                DoubleConstantNode dc => dc.Value,
                StringConstantNode sc => sc.Value,
                BooleanConstantNode bc => bc.Value,
                BinaryExpressionNode bn => bn.Evaluate(variables),
                TertiaryExpressionNode tn => tn.Evaluate(variables),
                VariableNode vn => vn.Evaluate(variables),
                ErrorNode en => en,
                _ => throw new ArgumentOutOfRangeException(nameof(node), node, null)
            };
        }

    }
}
