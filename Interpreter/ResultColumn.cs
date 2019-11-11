using System;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class ResultColumn : Result
    {
        public override ValueList Column { get; }

        public ResultColumn(ValueList column)
        {
            Column = column;
        }

        public override ResultSingle AggregationOperation(AggregationOp op) => new ResultSingle(op(Column));

        public override ResultSingle TransformOperation(TransformOp op) => new ResultSingle(op(Column));

        public override Result BinaryOperationTyped(BinaryOp op, ResultSingle right)
        {
            return new ResultColumn(
                new ValueList(Column.Select(v => op(v, right.Value)).ToList(), Column.AttributeType));
        }

        public override Result UnaryOperation(UnaryOp op)
        {
            return new ResultColumn(new ValueList(Column.Select(v => op(v)).ToList(), Column.AttributeType));
        }

        protected override Result CallMe(BinaryOp op, Result left)
        {
            return left switch
            {
                ResultColumn resultColumn => new ResultColumn(new ValueList(
                    resultColumn.Column.Zip(Column).Select(pair => op(pair.First, pair.Second)).ToList(),
                    Column.AttributeType)),
                ResultSingle resultSingle => new ResultColumn(
                    new ValueList(Column.Select(v => op(resultSingle.Value, v)).ToList(), Column.AttributeType)),
                ResultList resultList => throw new System.NotImplementedException(),
                _ => throw new Exception()
            };
        }

        public override Value Value => Column;
        public override ValueList List => throw new NotSupportedException("Not a ResultList");
        
        public override Result FilterNulls() => new ResultList(FilterNullList(Column));

        public override Result First(int size) => new ResultList(FirstList(Column, size));

        public override Result Last(int size) => new ResultList(LastList(Column, size));

        public override Result Random(int size) => new ResultList(RandomList(Column, size));

        public override Result ConvertTo(AttributeType to)
        {
            throw new System.NotImplementedException();
        }

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(Column.IsNull));
        public override AttributeType Type => Column.AttributeType;
    }
}