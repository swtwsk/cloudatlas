using System;
using System.Collections.Generic;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class Functions
    {
        public static Functions Instance => _instance ??= new Functions();

        private static Functions _instance;
        private readonly ValueTime _epoch;

        private Functions()
        {
            _epoch = new ValueTime("2000/01/01 00:00:00.000");
        }

        public Result Evaluate(string name, List<Result> arguments)
        {
            var size = arguments.Count;
            switch (name)
            {
                case "round":
                    if (size == 1)
                        return arguments[0].UnaryOperation(Round);
                    break;
                case "floor":
                    if (size == 1)
                        return arguments[0].UnaryOperation(Floor);
                    break;
                case "ceil":
                    if (size == 1)
                        return arguments[0].UnaryOperation(Ceil);
                    break;
                case "now":
                    if (size == 0)
                        return new ResultSingle(new ValueTime(DateTime.Now.GetTime()));
                    break;
                case "epoch":
                    if (size == 0)
                        return new ResultSingle(_epoch);
                    break;
                case "count":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Count);
                    break;
                case "size":
                    if (size == 1)
                        return arguments[0].ValueSize();
                    break;
                case "sum":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Sum);
                    break;
                case "avg":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Average);
                    break;
                case "land":
                    if (size == 1)
                        return arguments[0].AggregationOperation(And);
                    break;
                case "lor":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Or);
                    break;
                case "min":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Min);
                    break;
                case "max":
                    if (size == 1)
                        return arguments[0].AggregationOperation(Max);
                    break;
                case "unfold":
                    if (size == 1)
                        return arguments[0].TransformOperation(Unfold);
                    break;
                case "distinct":
                    if (size == 1)
                        return arguments[0].TransformOperation(Distinct);
                    break;
                case "sort":
                    if (size == 1)
                        return arguments[0].TransformOperation(Sort);
                    break;
                case "filterNulls":
                    if (size == 1)
                        return arguments[0].FilterNulls();
                    break;
                case "first":
                    if (size == 2)
                    {
                        var s = arguments[0].Value;
                        if (s.AttributeType.IsCompatible(AttributeTypePrimitive.Integer) &&
                            ((ValueInt) s).Value.Ref >= 0)
                            return arguments[1].First((int) ((ValueInt) s).Value.Ref);
                        throw new ArgumentException(
                            $"First argument must have type {AttributeTypePrimitive.Integer} and be >= 0.");
                    }
                    break;
                case "last":
                    if (size == 2)
                    {
                        var s = arguments[0].Value;
                        if (s.AttributeType.IsCompatible(AttributeTypePrimitive.Integer) &&
                            ((ValueInt) s).Value.Ref >= 0)
                            return arguments[1].Last((int) ((ValueInt) s).Value.Ref);
                        throw new ArgumentException("First argument must have type " + AttributeTypePrimitive.Integer
                                                                                     + " and be >= 0.");
                    }
                    break;
                case "random":
                    if (size == 2)
                    {
                        var s = arguments[0].Value;
                        if (s.AttributeType.IsCompatible(AttributeTypePrimitive.Integer) &&
                            ((ValueInt) s).Value.Ref >= 0)
                            return arguments[1].Random((int) ((ValueInt) s).Value.Ref);
                        throw new ArgumentException("First argument must have type " + AttributeTypePrimitive.Integer
                                                                                     + " and be >= 0.");
                    }
                    break;
                case "to_boolean":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Boolean);
                    break;
                case "to_contact":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Contact);
                    break;
                case "to_double":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Double);
                    break;
                case "to_duration":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Duration);
                    break;
                case "to_integer":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Integer);
                    break;
                case "to_string":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.String);
                    break;
                case "to_time":
                    if (size == 1)
                        return arguments[0].ConvertTo(AttributeTypePrimitive.Time);
                    break;
                case "to_set":
                    if (size == 1)
                    {
                        var t = arguments[0].Type;
                        if (!t.IsCollection())
                            throw new ArgumentException("First argument must be a collection.");
                        var elementType = ((AttributeTypeCollection) t).ElementType;
                        return arguments[0].ConvertTo(new AttributeTypeCollection(PrimaryType.Set, elementType));
                    }
                    break;
                case "to_list":
                    if (size == 1)
                    {
                        var t = arguments[0].Type;
                        if (!t.IsCollection())
                            throw new ArgumentException("First argument must be a collection.");
                        var elementType = ((AttributeTypeCollection) t).ElementType;
                        return arguments[0].ConvertTo(new AttributeTypeCollection(PrimaryType.List, elementType));
                    }
                    break;
                case "isNull":
                    if (size == 1)
                        return arguments[0].IsNull;
                    break;
                default:
                    throw new ArgumentException("Illegal function name.");
            }

            throw new ArgumentException("Illegal number of arguments.");
        }

        private static readonly Result.UnaryOp Round = v =>
        {
            if (!v.AttributeType.IsCompatible(AttributeTypePrimitive.Double))
                throw new ArgumentException($"Value must have type {AttributeTypePrimitive.Double}.");
            return v.IsNull ? new ValueDouble(null) : new ValueDouble(Math.Round(((ValueDouble) v).Value.Ref));
        };

        private static readonly Result.UnaryOp Floor = v =>
        {
            if (!v.AttributeType.IsCompatible(AttributeTypePrimitive.Double))
                throw new ArgumentException($"Value must have type {AttributeTypePrimitive.Double}.");
            return v.IsNull ? new ValueDouble(null) : new ValueDouble(Math.Floor(((ValueDouble) v).Value.Ref));
        };
        
        private static readonly Result.UnaryOp Ceil = v =>
        {
            if (!v.AttributeType.IsCompatible(AttributeTypePrimitive.Double))
                throw new ArgumentException($"Value must have type {AttributeTypePrimitive.Double}.");
            return v.IsNull ? new ValueDouble(null) : new ValueDouble(Math.Ceiling(((ValueDouble) v).Value.Ref));
        };

        private static readonly Result.AggregationOp Count = values =>
        {
            var nList = Result.FilterNullList(values);
            return nList.Value == null ? new ValueInt(null) : new ValueInt(nList.Value.Count);
        };

        private static readonly Result.AggregationOp Sum = values =>
        {
            var elementType = ((AttributeTypeCollection) values.AttributeType).ElementType;
            var primaryType = elementType.PrimaryType;
            
            if (primaryType != PrimaryType.Int && primaryType != PrimaryType.Double && primaryType != PrimaryType.Duration
                && primaryType != PrimaryType.Null)
                throw new ArgumentException($"Aggregation doesn't support type: {elementType}.");

            var nList = Result.FilterNullList(values);
            if (nList.Value == null || !nList.Any())
                return ValueNull.Instance;
            
            return nList.Value.Aggregate((a, b) => a.Add(b));
        };

        private static readonly Result.AggregationOp Average = values =>
        {
            var elementType = ((AttributeTypeCollection) values.AttributeType).ElementType;
            var primaryType = elementType.PrimaryType;

            if (primaryType != PrimaryType.Int && primaryType != PrimaryType.Double &&
                primaryType != PrimaryType.Duration
                && primaryType != PrimaryType.Null)
                throw new ArgumentException($"Aggregation doesn't support type: {elementType}.");

            var nList = Result.FilterNullList(values);
            if (nList.Value == null || !nList.Any())
                return ValueNull.Instance;

            var result = nList.Value.Aggregate((a, b) => a.Add(b));

            var size = primaryType == PrimaryType.Double
                ? new ValueDouble((double) ((ValueInt) nList.ValueSize()).Value.Ref)
                : nList.ValueSize();

            return result.Divide(size);
        };

        private static readonly Result.AggregationOp And = values =>
        {
            var nList = Result.FilterNullList(values);
            if (nList.Value == null)
                return new ValueBoolean(null);
            if (!values.Any())
                return new ValueBoolean(true);

            foreach (var v in nList) {
                if (v.AttributeType.IsCompatible(AttributeTypePrimitive.Boolean))
                {
                    if (v.IsNull || !((ValueBoolean) v).Value.Ref)
                        return new ValueBoolean(false);
                }
                else
                    throw new ArgumentException($"Aggregation doesn't support type: {v.AttributeType}.");
            }
            
            return new ValueBoolean(true);
        };
        
        private static readonly Result.AggregationOp Or = values =>
        {
            var nList = Result.FilterNullList(values);
            if (nList.Value == null)
                return new ValueBoolean(null);
            if (!values.Any())
                return new ValueBoolean(false);

            foreach (var v in nList) {
                if (v.AttributeType.IsCompatible(AttributeTypePrimitive.Boolean))
                {
                    if (!v.IsNull && ((ValueBoolean) v).Value.Ref)
                        return new ValueBoolean(true);
                }
                else
                    throw new ArgumentException($"Aggregation doesn't support type: {v.AttributeType}.");
            }
            
            return new ValueBoolean(false);
        };

        private static readonly Result.AggregationOp Min = values =>
        {
            var nList = Result.FilterNullList(values);
            if (nList.Value == null || !nList.Any())
                return ValueNull.Instance;
            
            return nList.Aggregate((a, b) => ((ValueBoolean) a.IsLowerThan(b)).Value.Ref ? a : b);
        };
        
        private static readonly Result.AggregationOp Max = values => 
        {
            var nList = Result.FilterNullList(values);
            if (nList.Value == null || !nList.Any())
                return ValueNull.Instance;

            bool IsGreaterThan(Value a, Value b) =>
                ((ValueBoolean) a.IsLowerThan(b).Negate().And(a.IsEqual(b).Negate())).Value.Ref;

            return nList.Aggregate((a, b) => IsGreaterThan(a, b) ? a : b);
        };

        private static readonly Result.TransformOp Unfold = values =>
        {
            var nList = Result.FilterNullList(values);
            var elementType = ((AttributeTypeCollection) nList.AttributeType).ElementType;
            if (nList.Value == null || !nList.Any())
                return new ValueList(elementType);

            var unfolded = nList.SelectMany(v =>
            {
                return v.AttributeType.PrimaryType switch
                {
                    PrimaryType.List => ((ValueList) v).ToList(),
                    PrimaryType.Set => ((ValueSet) v).ToList(),
                    _ => throw new ArgumentException($"Unfolding should work on lists, {v.AttributeType} isn't such.")
                };
            });
            return new ValueList(unfolded.ToList(), elementType);
        };

        private static readonly Result.TransformOp Distinct = values =>
        {
            var nList = Result.FilterNullList(values);
            var elementType = ((AttributeTypeCollection) nList.AttributeType).ElementType;
            if (nList.Value == null || !nList.Any())
                return new ValueList(elementType);
            return new ValueList(nList.Distinct().ToList(), elementType);
        };

        private static readonly Result.TransformOp Sort = values =>
        {
            Func<Value, Value, int> comparer = (a, b) =>
            {
                if (a.IsNull)
                    return b.IsNull ? 0 : -1;

                if (b.IsNull)
                    return 1;

                return a.IsEqual(b).GetBoolean() ? 0 :
                    a.IsLowerThan(b).GetBoolean() ? -1 : 1;
            };

            var toSort = values.Value.ToList();
            toSort.Sort(Compare.By(comparer));
            
            return new ValueList(toSort, ((AttributeTypeCollection) values.AttributeType).ElementType);
        };
    }
}