using System;
using System.Collections.Generic;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class ResultSingle : Result
    {
        public override Value Value { get; }

        public ResultSingle(Value value) => Value = value;

        public override Result BinaryOperationTyped(BinaryOp op, ResultSingle right) =>
            new ResultSingle(op(Value, right.Value));

        public override Result UnaryOperation(UnaryOp op) => new ResultSingle(op(Value));

        protected override Result CallMe(BinaryOp op, Result left) => left.BinaryOperationTyped(op, this);

        public override ValueList List => throw new NotSupportedException("Not a ResultList");
        public override ValueList Column => throw new NotSupportedException("Not a ResultColumn");

        public override ResultSingle AggregationOperation(AggregationOp op) =>
            throw new NotSupportedException("Aggregation Operations not supported on ResultSingle.");

        public override Result TransformOperation(TransformOp op) =>
            throw new NotSupportedException("Transform Operations not supported on ResultSingle.");

        public override Result FilterNulls() =>
            throw new NotSupportedException("Operation filterNulls not supported on ResultSingle.");

        public override Result First(int size) =>
            throw new NotSupportedException("Operation first not supported on ResultSingle.");

        public override Result Last(int size) =>
            throw new NotSupportedException("Operation last not supported on ResultSingle.");

        public override Result Random(int size) =>
            throw new NotSupportedException("Operation random not supported on ResultSingle.");

        public override Result ConvertTo(AttributeType to) => new ResultSingle(Value.ConvertTo(to));

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(Value.IsNull));
        public override AttributeType Type => Value.AttributeType;
    }
}