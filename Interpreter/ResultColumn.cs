using System;
using System.Collections.Generic;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class ResultColumn : Result
    {
        public override ValueList Column { get; }
        private AttributeType ElementType => ((AttributeTypeCollection) Column.AttributeType).ElementType;

        public ResultColumn(ValueList column)
        {
            Column = column;
        }

        public override ResultSingle AggregationOperation(AggregationOp op) => new ResultSingle(op(Column));

        public override Result TransformOperation(TransformOp op) => new ResultList(op(Column));

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
        
        public override Result FilterNulls() => new ResultList(FilterNullList(Column));

        public override Result First(int size) => new ResultSingle(FirstList(Column, size));

        public override Result Last(int size) => new ResultSingle(LastList(Column, size));

        public override Result Random(int size) => new ResultSingle(RandomList(Column, size));

        public override Result ConvertTo(AttributeType to)
        {
            return new ResultColumn(new ValueList(Column.Select(v => v.ConvertTo(to)).ToList(), to));
        }

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(Column.IsNull));
        public override AttributeType Type => Column.AttributeType;
    }
}