using System;
using System.Collections.Generic;

namespace DTOMaker.Gentime
{
    public sealed class ExprParser : Parser<ExprToken, Node>
    {
        // -------------------- start of grammar --------------------------------------
        private static ParserResult<Node> MatchSymbol(ParserInputs<ExprToken> input, ExprToken symbol)
        {
            var tokens = input.Source.Span;
            if (tokens.Length == 0) return default;
            if (tokens[0].Kind != symbol) return default;
            return new ParserResult<Node>(1, SymbolConstantNode.Create(tokens[0]));
        }

        private static ParserResult<Node> LeftParen(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.LParen);
        private static ParserResult<Node> RightParen(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.RParen);
        private static ParserResult<Node> PowerSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Power);
        private static ParserResult<Node> MultiplySymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Star);
        private static ParserResult<Node> DivideSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Slash);
        private static ParserResult<Node> ModuloSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Percent);
        private static ParserResult<Node> AddSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Plus);
        private static ParserResult<Node> SubtractSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Dash);
        private static ParserResult<Node> GreaterThanSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.RAngle);
        private static ParserResult<Node> GreaterThanEqualSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.GEQ);
        private static ParserResult<Node> LessThanSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.LAngle);
        private static ParserResult<Node> LessThanEqualSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.LEQ);
        private static ParserResult<Node> EqualToSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.EQU);
        private static ParserResult<Node> NotEqualToSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.NEQ);
        private static ParserResult<Node> IfThenSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Quest);
        private static ParserResult<Node> ElseSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Colon);
        private static ParserResult<Node> AndSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.AND);
        private static ParserResult<Node> OrSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.OR);

        private static ParserResult<Node> MultiplicativeOperator(ParserInputs<ExprToken> input)
        {
            return input.FirstOf(
                MultiplySymbol,
                DivideSymbol,
                ModuloSymbol);
        }

        private static ParserResult<Node> AdditiveOperator(ParserInputs<ExprToken> input)
        {
            return input.FirstOf(AddSymbol, SubtractSymbol);
        }

        private static ParserResult<Node> RelationalOperator(ParserInputs<ExprToken> input)
        {
            return input.FirstOf(
                GreaterThanSymbol,
                GreaterThanEqualSymbol,
                LessThanSymbol,
                LessThanEqualSymbol);
        }

        private static ParserResult<Node> EqualityOperator(ParserInputs<ExprToken> input)
        {
            return input.FirstOf(
                EqualToSymbol,
                NotEqualToSymbol);
        }

        private static ParserResult<Node> MatchConstant(ParserInputs<ExprToken> input, ExprToken kind, Func<ReadOnlyMemory<char>, Node> nodeFunc)
        {
            var tokens = input.Source.Span;
            if (tokens.Length == 0) return default;
            if (tokens[0].Kind != kind) return default;
            return new ParserResult<Node>(1, nodeFunc(tokens[0].Source));
        }

        private static ParserResult<Node> NullConstant(ParserInputs<ExprToken> input) => MatchConstant(input, ExprToken.Null, NullConstantNode.Create);
        private static ParserResult<Node> BooleanConstant(ParserInputs<ExprToken> input) => MatchConstant(input, ExprToken.Bool, BooleanConstantNode.Create);
        private static ParserResult<Node> StringConstant(ParserInputs<ExprToken> input) => MatchConstant(input, ExprToken.Str, StringConstantNode.Create);
        private static ParserResult<Node> NumberConstant(ParserInputs<ExprToken> input) => MatchConstant(input, ExprToken.Num, NumericConstantNode.Create);
        private static ParserResult<Node> AnyConstant(ParserInputs<ExprToken> input) => input.FirstOf(NullConstant, BooleanConstant, StringConstant, NumberConstant);

        private static Node BinaryExpressionBuilder(Node left, Node symbol, Node right)
        {
            return new BinaryExpressionNode() { Left = left, Op = symbol.ToBinaryOperator(), Right = right };
        }

        // -------------------- precedence 0 ----------------------------------------
        private static ParserResult<Node> GroupExpression(ParserInputs<ExprToken> input) => Combinators.AllOf(input, LeftParen, AnyExpression, RightParen, (left, expr, right) => expr);
        private static ParserResult<Node> Precedence0(ParserInputs<ExprToken> input) => input.FirstOf(VariableName, AnyConstant, GroupExpression);

        // -------------------- precedence 1 ----------------------------------------
        private static ParserResult<Node> PowerExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence0, PowerSymbol, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence1(ParserInputs<ExprToken> input) => input.FirstOf(PowerExpression, Precedence0);

        // -------------------- precedence 2 ----------------------------------------
        private static ParserResult<Node> MultiplyExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence1, MultiplicativeOperator, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence2(ParserInputs<ExprToken> input) => input.FirstOf(MultiplyExpression, Precedence1);

        // -------------------- precedence 3 ----------------------------------------
        private static ParserResult<Node> AdditiveExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence2, AdditiveOperator, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence3(ParserInputs<ExprToken> input) => input.FirstOf(AdditiveExpression, Precedence2);

        // -------------------- precedence 4 ----------------------------------------
        private static ParserResult<Node> RelationalExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence3, RelationalOperator, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence4(ParserInputs<ExprToken> input) => input.FirstOf(RelationalExpression, Precedence3);

        // -------------------- precedence 5 ----------------------------------------
        private static ParserResult<Node> EqualityExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence4, EqualityOperator, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence5(ParserInputs<ExprToken> input) => input.FirstOf(EqualityExpression, Precedence4);

        // -------------------- precedence 6 ----------------------------------------
        private static ParserResult<Node> AndExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence5, AndSymbol, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence6(ParserInputs<ExprToken> input) => input.FirstOf(AndExpression, Precedence5);

        // -------------------- precedence 7 ----------------------------------------
        private static ParserResult<Node> OrExpression(ParserInputs<ExprToken> input) => Combinators.Chain(input, Precedence6, OrSymbol, BinaryExpressionBuilder);
        private static ParserResult<Node> Precedence7(ParserInputs<ExprToken> input) => input.FirstOf(OrExpression, Precedence6);

        // -------------------- precedence 8 ----------------------------------------
        private static ParserResult<Node> IfThenElseExpression(ParserInputs<ExprToken> input)
        {
            return Combinators.AllOf(input, Precedence7, IfThenSymbol, Precedence7, ElseSymbol, Precedence7,
                (ifExpr, notUsed1, thenExpr, notUsed2, elseExpr) => TertiaryExpressionNode.Create(TertiaryOperator.IfThenElse, ifExpr, thenExpr, elseExpr));
        }

        private static ParserResult<Node> Precedence8(ParserInputs<ExprToken> input) => input.FirstOf(IfThenElseExpression, Precedence7);

        // -------------------- precedence 9 ----------------------------------------
        private static ParserResult<Node> AssignSymbol(ParserInputs<ExprToken> input) => MatchSymbol(input, ExprToken.Assign);
        private static ParserResult<Node> VariableName(ParserInputs<ExprToken> input)
        {
            var tokens = input.Source.Span;
            if (tokens.Length == 0) return default;
            if (tokens[0].Kind != ExprToken.Var) return default;
            return new ParserResult<Node>(1, VariableNode.Create(new string(tokens[0].Source.ToArray())));
        }

        private static ParserResult<Node> AssignExpression(ParserInputs<ExprToken> input)
        {
            return Combinators.AllOf(input, VariableName, AssignSymbol, Precedence8,
                (varName, symbol, expr) => BinaryExpressionNode.Create(BinaryOperator.Assign, varName, expr));
        }

        private static ParserResult<Node> Precedence9(ParserInputs<ExprToken> input) => input.FirstOf(AssignExpression, Precedence8);

        // -------------------- least precedent ---------------------------------------

        private static ParserResult<Node> AnyExpression(ParserInputs<ExprToken> input) => Precedence9(input);

        // -------------------- end of grammar ----------------------------------------

        public ExprParser() : base(new ExprLexer()) { }
        protected override IEnumerable<Token<ExprToken>> OnFilterTokens(IEnumerable<Token<ExprToken>> tokens) => tokens.SelectCodeTokens();
        protected override Node OnMakeErrorNode(string message) => new ErrorNode() { Message = message };
        protected override bool OnTryMatch(ReadOnlyMemory<Token<ExprToken>> tokens, out int consumed, out Node? result)
        {
            ParserInputs<ExprToken> inputs = new ParserInputs<ExprToken>(tokens);
            var parsed = AnyExpression(inputs);
            consumed = parsed.Consumed;
            result = parsed.Result;
            return parsed.Matched;
        }
    }
}
