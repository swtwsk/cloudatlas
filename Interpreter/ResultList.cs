using System;
using System.Collections.Generic;
using System.Linq;
using CloudAtlas.Model;
using CloudAtlas.Monads;

namespace CloudAtlas.Interpreter
{
    public class ResultList : Result
    {
        public ResultList(ValueList list) => List = list;
        private AttributeType ElementType => ((AttributeTypeCollection) List.AttributeType).ElementType;

        public override Value Value => List;
        public override ValueList List { get; }
        public override ValueList Column => throw new NotSupportedException("Not a ResultColumn");
        
        public override Result BinaryOperationTyped(BinaryOp op, ResultSingle right)
        {
            return new ResultColumn(
                new ValueList(List.Select(v => op(v, right.Value)).ToList(), ElementType));
        }

        public override Result UnaryOperation(UnaryOp op)
        {
            return new ResultColumn(new ValueList(List.Select(v => op(v)).ToList(), ElementType));
        }

        protected override Result CallMe(BinaryOp op, Result left)
        {
            return left switch
            {
                ResultSingle resultSingle => new ResultColumn(
                    new ValueList(List.Select(v => op(resultSingle.Value, v)).ToList(), ElementType)),
                _ => throw new NotSupportedException(
                    $"Cannot do BinaryOp on ResultList and something that's not a ResultSingle")
            };
        }

        public override Maybe<ResultSingle> AggregationOperation(AggregationOp op) => new ResultSingle(op(List)).Just();
        public override Maybe<Result> TransformOperation(TransformOp op) => Maybe<Result>.Just(new ResultList(op(List)));

        public override Maybe<Result> FilterNulls() => (new ResultList(FilterNullList(List)) as Result).Just();
        public override Maybe<Result> First(int size) => (new ResultSingle(FirstList(List, size)) as Result).Just();
        public override Maybe<Result> Last(int size) => (new ResultSingle(LastList(List, size)) as Result).Just();
        public override Maybe<Result> Random(int size) => (new ResultSingle(RandomList(List, size)) as Result).Just();

        public override Result ConvertTo(AttributeType to)
        {
            return new ResultList(new ValueList(List.Select(v => v.ConvertTo(to)).ToList(), to));
        }

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(List.IsNull));
        public override AttributeType Type => List.AttributeType;
    }
}