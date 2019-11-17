using System;
using System.Linq;
using Shared.Model;
using Shared.Monads;

namespace Interpreter
{
    public class ResultColumn : Result
    {
        public override ValueList Column { get; }
        private AttributeType ElementType => ((AttributeTypeCollection) Column.AttributeType).ElementType;

        public ResultColumn(ValueList column)
        {
            Column = column;
        }

        public override Maybe<ResultSingle> AggregationOperation(AggregationOp op) => new ResultSingle(op(Column)).Just();

        public override Maybe<Result> TransformOperation(TransformOp op) => Maybe<Result>.Just(new ResultList(op(Column)));

        public override Result BinaryOperationTyped(BinaryOp op, ResultSingle right)
        {
            return new ResultColumn(new ValueList(Column.Select(v => op(v, right.Value)).ToList(), ElementType));
        }

        public override Result UnaryOperation(UnaryOp op)
        {
            return new ResultColumn(new ValueList(Column.Select(v => op(v)).ToList(), ElementType));
        }

        protected override Result CallMe(BinaryOp op, Result left)
        {
            return left switch
            {
                ResultColumn resultColumn => new ResultColumn(new ValueList(
                    resultColumn.Column.Zip(Column).Select(pair => op(pair.First, pair.Second)).ToList(),
                    ElementType)),
                ResultSingle resultSingle => new ResultColumn(
                    new ValueList(Column.Select(v => op(resultSingle.Value, v)).ToList(), ElementType)),
                _ => throw new NotSupportedException($"Cannot do BinaryOp on ResultColumn and {left}")
            };
        }

        public override Value Value => Column;
        public override ValueList List => throw new NotSupportedException("Not a ResultList");
        
        public override Maybe<Result> FilterNulls() => (new ResultList(FilterNullList(Column)) as Result).Just();

        public override Maybe<Result> First(int size) => (new ResultSingle(FirstList(Column, size)) as Result).Just();

        public override Maybe<Result> Last(int size) => (new ResultSingle(LastList(Column, size)) as Result).Just();

        public override Maybe<Result> Random(int size) => (new ResultSingle(RandomList(Column, size)) as Result).Just();

        public override Result ConvertTo(AttributeType to)
        {
            return new ResultColumn(new ValueList(Column.Select(v => v.ConvertTo(to)).ToList(), to));
        }

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(Column.IsNull));
        public override AttributeType Type => Column.AttributeType;
    }
}