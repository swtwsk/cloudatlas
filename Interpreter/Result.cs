using System;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public abstract class Result
    {
        public delegate Value BinaryOp(Value v1, Value v2);
        public delegate Value UnaryOp(Value v);
        public delegate Value AggregationOp(ValueList values);
        public delegate ValueList TransformOp(ValueList values);

        private static readonly BinaryOp IS_EQUAL = (v1, v2) => v1.IsEqual(v2);
        private static readonly BinaryOp IS_LOWER_THAN = (v1, v2) => v1.IsLowerThan(v2);
        private static readonly BinaryOp ADD = (v1, v2) => v1.Add(v2);
        private static readonly BinaryOp SUBTRACT = (v1, v2) => v1.Subtract(v2);
        private static readonly BinaryOp MULTIPLY = (v1, v2) => v1.Multiply(v2);
        private static readonly BinaryOp DIVIDE = (v1, v2) => v1.Divide(v2);
        private static readonly BinaryOp MODULO = (v1, v2) => v1.Modulo(v2);
        private static readonly BinaryOp AND = (v1, v2) => v1.And(v2);
        private static readonly BinaryOp OR = (v1, v2) => v1.Or(v2);
        private static readonly BinaryOp REG_EXPR = (v1, v2) => v1.RegExpr(v2);

        private static readonly UnaryOp NEGATE = v => v.Negate();
        private static readonly UnaryOp VALUE_SIZE = v => v.ValueSize();

        public abstract Result BinaryOperationTyped(BinaryOp op, ResultSingle right);

        public Result BinaryOperation(BinaryOp op, Result right) => right.CallMe(op, this);

        public abstract Result UnaryOperation(UnaryOp op);

        protected abstract Result CallMe(BinaryOp op, Result left);
        
        public abstract Value Value { get; }
        public abstract ValueList List { get; }
        public abstract ValueList Column { get; }

        public ResultSingle AggregationOperation(AggregationOp op) => throw new NotImplementedException();

        public ResultSingle TransformOperation(TransformOp op) => throw new NotImplementedException();

        public Result IsEqual(Result right) => right.CallMe(IS_EQUAL, this);
        public Result IsLowerThan(Result right) => right.CallMe(IS_LOWER_THAN, this);
        public Result Add(Result right) => right.CallMe(ADD, this);
        public Result Subtract(Result right) => right.CallMe(SUBTRACT, this);
        public Result Multiply(Result right) => right.CallMe(MULTIPLY, this);
        public Result Divide(Result right) => right.CallMe(DIVIDE, this);
        public Result Modulo(Result right) => right.CallMe(MODULO, this);
        public Result And(Result right) => right.CallMe(AND, this);
        public Result Or(Result right) => right.CallMe(OR, this);
        public Result RegExpr(Result right) => right.CallMe(REG_EXPR, this);
        public Result Negate() => UnaryOperation(NEGATE);
        public Result ValueSize() => UnaryOperation(VALUE_SIZE);

        public static ValueList FilterNullList(ValueList list)
        {
            var elementType = ((AttributeTypeCollection) list.AttributeType).ElementType;
            if (!list.Any())
                return new ValueList(elementType);
            var result = list.Where(v => !v.IsNull).ToList();
            return new ValueList(result.Any() ? result : null, elementType);
        }

        public abstract Result FilterNulls();

        protected static ValueList FirstList(ValueList list, int size)
        {
            var nList = FilterNullList(list);
            return nList.Value == null
                ? nList
                : new ValueList(nList.Take(size).ToList(), ((AttributeTypeCollection) list.AttributeType).ElementType);
        }

        public abstract Result First(int size);

        protected static ValueList LastList(ValueList list, int size)
        {
            var nList = FilterNullList(list);
            return nList.Value == null
                ? nList
                : new ValueList(nList.TakeLast(size).ToList(),
                    ((AttributeTypeCollection) list.AttributeType).ElementType);
        }

        public abstract Result Last(int size);

        protected static ValueList RandomList(ValueList list, int size)
        {
            var nList = FilterNullList(list);
            if (nList.Value == null || nList.Count <= size)
                return nList;
            nList.Shuffle();
            return new ValueList(nList.Value.Take(size).ToList(), ((AttributeTypeCollection) list.AttributeType).ElementType);
        }

        public abstract Result Random(int size);

        public abstract Result ConvertTo(AttributeType to);

        public abstract ResultSingle IsNull { get; }

        public abstract AttributeType Type { get; }
    }
}