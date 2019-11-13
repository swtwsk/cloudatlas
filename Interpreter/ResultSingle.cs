using System;
using CloudAtlas.Model;
using CloudAtlas.Monads;

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

        public override Maybe<ResultSingle> AggregationOperation(AggregationOp op) =>
            Value.IsNull
                ? new ResultSingle(ValueNull.Instance).Just()
                : throw new NotSupportedException("Aggregation Operations not supported on ResultSingle.");

        public override Maybe<Result> TransformOperation(TransformOp op) => Value.IsNull
            ? (new ResultSingle(ValueNull.Instance) as Result).Just()
            : throw new NotSupportedException("Transform Operations not supported on ResultSingle.");

        public override Maybe<Result> FilterNulls() =>
            Value.IsNull
                ? Maybe<Result>.Nothing
                : throw new NotSupportedException("Operation filterNulls not supported on ResultSingle.");

        public override Maybe<Result> First(int size) =>
            Value.IsNull
                ? Maybe<Result>.Nothing
                : throw new NotSupportedException("Operation first not supported on ResultSingle.");

        public override Maybe<Result> Last(int size) =>
            Value.IsNull
                ? Maybe<Result>.Nothing
                : throw new NotSupportedException("Operation last not supported on ResultSingle.");

        public override Maybe<Result> Random(int size) =>
            Value.IsNull
                ? Maybe<Result>.Nothing
                : throw new NotSupportedException("Operation random not supported on ResultSingle.");

        public override Result ConvertTo(AttributeType to) => new ResultSingle(Value.ConvertTo(to));

        public override ResultSingle IsNull => new ResultSingle(new ValueBoolean(Value.IsNull));
        public override AttributeType Type => Value.AttributeType;
    }
}