using System;

namespace DTOMaker.Gentime
{
    public static class Combinators
    {
        public static ParserResult<TNode> FirstOf<TEnum, TNode>(this ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var result = parserA(input);
            if (result.Matched) return result;
            result = parserB(input);
            if (result.Matched) return result;
            return default;
        }

        public static ParserResult<TNode> FirstOf<TEnum, TNode>(this ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserC
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var result = parserA(input);
            if (result.Matched) return result;
            result = parserB(input);
            if (result.Matched) return result;
            result = parserC(input);
            if (result.Matched) return result;
            return default;
        }

        public static ParserResult<TNode> FirstOf<TEnum, TNode>(this ParserInputs<TEnum> input, params Func<ParserInputs<TEnum>, ParserResult<TNode>>[] parsers) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            if (parsers.Length == 0) return default;
            foreach (var parser in parsers)
            {
                var result = parser(input);
                if (result.Matched) return result;
            }
            return default;
        }

        public static ParserResult<TNode> AllOf<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB,
            Func<TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            var resultA = parserA(remaining);
            if (!resultA.Matched) return default;
            remaining = remaining.Consume(resultA.Consumed);
            var resultB = parserB(remaining);
            if (!resultB.Matched) return default;
            // success!
            return new ParserResult<TNode>(
                resultA.Consumed + resultB.Consumed,
                combiner(resultA.Result, resultB.Result));
        }

        public static ParserResult<TNode> AllOf<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserC,
            Func<TNode, TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            var resultA = parserA(remaining);
            if (!resultA.Matched) return default;
            remaining = remaining.Consume(resultA.Consumed);
            var resultB = parserB(remaining);
            if (!resultB.Matched) return default;
            remaining = remaining.Consume(resultB.Consumed);
            var resultC = parserC(remaining);
            if (!resultC.Matched) return default;
            // success!
            return new ParserResult<TNode>(
                resultA.Consumed + resultB.Consumed + resultC.Consumed,
                combiner(resultA.Result, resultB.Result, resultC.Result));
        }

        public static ParserResult<TNode> AllOf<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserC,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserD,
            Func<TNode, TNode, TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            var resultA = parserA(remaining);
            if (!resultA.Matched) return default;
            remaining = remaining.Consume(resultA.Consumed);
            var resultB = parserB(remaining);
            if (!resultB.Matched) return default;
            remaining = remaining.Consume(resultB.Consumed);
            var resultC = parserC(remaining);
            if (!resultC.Matched) return default;
            remaining = remaining.Consume(resultC.Consumed);
            var resultD = parserD(remaining);
            if (!resultC.Matched) return default;
            // success!
            return new ParserResult<TNode>(
                resultA.Consumed + resultB.Consumed + resultC.Consumed + resultD.Consumed,
                combiner(resultA.Result, resultB.Result, resultC.Result, resultD.Result));
        }

        public static ParserResult<TNode> AllOf<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserA,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserB,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserC,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserD,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> parserE,
            Func<TNode, TNode, TNode, TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            var resultA = parserA(remaining);
            if (!resultA.Matched) return default;
            remaining = remaining.Consume(resultA.Consumed);
            var resultB = parserB(remaining);
            if (!resultB.Matched) return default;
            remaining = remaining.Consume(resultB.Consumed);
            var resultC = parserC(remaining);
            if (!resultC.Matched) return default;
            remaining = remaining.Consume(resultC.Consumed);
            var resultD = parserD(remaining);
            if (!resultC.Matched) return default;
            remaining = remaining.Consume(resultD.Consumed);
            var resultE = parserE(remaining);
            if (!resultC.Matched) return default;
            // success!
            return new ParserResult<TNode>(
                resultA.Consumed + resultB.Consumed + resultC.Consumed + resultD.Consumed + resultE.Consumed,
                combiner(resultA.Result, resultB.Result, resultC.Result, resultD.Result, resultE.Result));
        }

        /// <summary>
        /// Parses 3 or more tokens matching the pattern: X op X op X ...
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="input"></param>
        /// <param name="leftRightParser"></param>
        /// <param name="operatorParser"></param>
        /// <param name="combiner"></param>
        /// <returns></returns>
        public static ParserResult<TNode> Chain<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> leftRightParser,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> operatorParser,
            Func<TNode, TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            // first match
            // - left
            var left = leftRightParser(remaining);
            if (!left.Matched) return default;
            remaining = remaining.Consume(left.Consumed);
            // - op
            var op = operatorParser(remaining);
            if (!op.Matched) return default;
            remaining = remaining.Consume(op.Consumed);
            // - right
            var right = leftRightParser(remaining);
            if (!right.Matched) return default;
            remaining = remaining.Consume(right.Consumed);
            // we have at least 1 node
            int consumed = left.Consumed + op.Consumed + right.Consumed;
            TNode node = combiner(left.Result, op.Result, right.Result);
            // more matches
            while (true)
            {
                // op
                op = operatorParser(remaining);
                if (!op.Matched) return new ParserResult<TNode>(consumed, node);
                remaining = remaining.Consume(op.Consumed);
                // node
                right = leftRightParser(remaining);
                if (!right.Matched) return new ParserResult<TNode>(consumed, node);
                remaining = remaining.Consume(right.Consumed);
                // next
                consumed += op.Consumed + right.Consumed;
                node = combiner(node, op.Result, right.Result);
            }
        }
        /// <summary>
        /// Parses 1 or more tokens matching the pattern: X op X op X ...
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="input"></param>
        /// <param name="leftRightParser"></param>
        /// <param name="operatorParser"></param>
        /// <param name="combiner"></param>
        /// <returns></returns>
        public static ParserResult<TNode> OneOrMore<TEnum, TNode>(ParserInputs<TEnum> input,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> leftRightParser,
            Func<ParserInputs<TEnum>, ParserResult<TNode>> operatorParser,
            Func<TNode, TNode, TNode, TNode> combiner
            ) where TEnum : struct
        {
            if (input.Source.Length == 0) return default;
            var remaining = input;
            // first match
            // - left
            var left = leftRightParser(remaining);
            if (!left.Matched) return default;
            remaining = remaining.Consume(left.Consumed);
            // we have at least 1 node
            int consumed = left.Consumed;
            TNode node = left.Result;
            // more matches
            while (true)
            {
                // op
                var op = operatorParser(remaining);
                if (!op.Matched) return new ParserResult<TNode>(consumed, node);
                remaining = remaining.Consume(op.Consumed);
                // node
                var right = leftRightParser(remaining);
                if (!right.Matched) return new ParserResult<TNode>(consumed, node);
                remaining = remaining.Consume(right.Consumed);
                // next
                consumed += op.Consumed + right.Consumed;
                node = combiner(node, op.Result, right.Result);
            }
        }
    }
}
