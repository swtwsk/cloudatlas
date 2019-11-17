using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime.Tree;
using Shared;
using Shared.Interpreter.Exceptions;
using Shared.Model;
using Shared.Monads;
using Attribute = Shared.Model.Attribute;

namespace Interpreter.Query
{
    public class QueryVisitor : QueryBaseVisitor<IEnumerable<Maybe<QueryResult>>>
    {
        private readonly ZMI _zmi;

        public QueryVisitor(ZMI zmi) => _zmi = zmi;

        public override IEnumerable<Maybe<QueryResult>> VisitProgram(QueryParser.ProgramContext context) =>
            context.statement().SelectMany(VisitStatement); // TODO: Handle errors

        public override IEnumerable<Maybe<QueryResult>> VisitStatement(QueryParser.StatementContext context)
        {
            var table = new Table(_zmi);
            if (context.where_clause() != null)
                table = new WhereVisitor(_zmi, table).Visit(context.where_clause());
            if (context.order_by_clause() != null)
                table = new OrderByVisitor(_zmi, table).Visit(context.order_by_clause());

            var ret = new List<Maybe<QueryResult>>();

            foreach (var selItem in context.sel_item())
            {
                var qr = new SelItemVisitor(_zmi, table).Visit(selItem);
                if (qr.HasValue)
                {
                    var qrV = qr.Val;
                    if (qrV.Name != null)
                    {
                        if (ret.Any(qrRet => qrRet.HasValue && qrV.Name.Name.Equals(qrRet.Val.Name.Name)))
                        {
                            throw new ArgumentException("Alias collision");
                        }
                    }
                }

                ret.Add(qr);
            }

            return ret;
        }
    }

    public class WhereVisitor : QueryBaseVisitor<Table>
    {
        private readonly Table _table;
        private readonly ZMI _zmi;

        public WhereVisitor(ZMI zmi, Table table)
        {
            _zmi = zmi;
            _table = table;
        }

        public override Table VisitWhere_clause(QueryParser.Where_clauseContext context)
        {
            var result = new Table(_table);
            foreach (var row in _table)
            {
                var env = new Environment(row, _table.Columns);
                var value = new CondExprVisitor(_zmi, env).Visit(context.cond_expr());
                if (value.HasValue && value.Val.Value.GetBoolean())
                    result.AppendRow(row);
            }

            return result;
        }
    }

    public class OrderByVisitor : QueryBaseVisitor<Table>
    {
        private Table _table;
        private readonly ZMI _zmi;

        public OrderByVisitor(ZMI zmi, Table table)
        {
            _zmi = zmi;
            _table = table;
        }

        public override Table VisitOrder_by_clause(QueryParser.Order_by_clauseContext context)
        {
            foreach (var item in context.order_item())
                _table = new OrderItemVisitor(_zmi, _table).Visit(item);

            return _table;
        }
    }

    public class OrderItemVisitor : QueryBaseVisitor<Table>
    {
        private readonly Table _table;
        private readonly ZMI _zmi;

        public OrderItemVisitor(ZMI zmi, Table table)
        {
            _zmi = zmi;
            _table = table;
        }

        public override Table VisitOrder_item(QueryParser.Order_itemContext context)
        {
            int Comparer(TableRow row1, TableRow row2)
            {
                var env1 = new Environment(row1, _table.Columns);
                var expr1 = new CondExprVisitor(_zmi, env1).Visit(context.cond_expr());
                var env2 = new Environment(row2, _table.Columns);
                var expr2 = new CondExprVisitor(_zmi, env2).Visit(context.cond_expr());
                var result = expr1.Zip(expr2).Bind(p =>
                {
                    var res = new NullsVisitor(p).VisitNulls(context.nulls());
                    if (res == 0)
                        return new OrderVisitor(p).VisitOrder(context.order()).Just();
                    return res.Just();
                });
                return result.Match(i => i, () => 0);
            }

            _table.Sort(Compare.By<TableRow>(Comparer));
            return _table;
        }
    }

    public class OrderVisitor : QueryBaseVisitor<int>
    {
        private readonly (Result, Result) _pair;

        public OrderVisitor((Result, Result) pair) => _pair = pair;

        public override int VisitOrder(QueryParser.OrderContext context) =>
            context?.DESC() == null ? CompareAsc(_pair) : -CompareAsc(_pair);

        private static int CompareAsc((Result, Result) pair)
        {
            var (left, right) = pair;
            return left.IsEqual(right).Value.GetBoolean() ? 0 :
                left.IsLowerThan(right).Value.GetBoolean() ? -1 : 1;
        }
    }

    public class NullsVisitor : QueryBaseVisitor<int>
    {
        private readonly (Result, Result) _pair;

        public NullsVisitor((Result, Result) pair) => _pair = pair;

        public override int VisitNulls(QueryParser.NullsContext context) =>
            context?.LAST() == null ? NullsFirst(_pair) : -NullsFirst(_pair);

        private static int NullsFirst((Result, Result) pair)
        {
            var (left, right) = pair;
            if (left.Value.IsNull)
                return (right.Value.IsNull ? 0 : -1);
            return (right.Value.IsNull ? 1 : 0);
        }
    }

    public class SelItemVisitor : QueryBaseVisitor<Maybe<QueryResult>>
    {
        private readonly ZMI _zmi;
        private readonly Table _table;

        public SelItemVisitor(ZMI zmi, Table table)
        {
            _zmi = zmi;
            _table = table;
        }

        public override Maybe<QueryResult> VisitSel_item(QueryParser.Sel_itemContext context)
        {
            var distinct = context.sel_modifier()?.DISTINCT() != null;
            var alias = context.identifier();

            var dict = new Dictionary<Attribute, List<Value>>();
            foreach (var son in _zmi.Sons)
            {
                foreach (var (k, v) in son.Attributes)
                {
                    if (dict.TryGetValue(k, out var l))
                        l.Add(v);
                    else
                        dict.Add(k, new List<Value> {v});
                }
            }

            var (attributes, values) = _table.Columns
                .Where(c => !Attribute.IsQuery(c))
                .Select(c => (key: c, value: _table.GetColumn(c)))
                .ToList()
                .Unzip(pair => pair.key, pair => pair.value);

            var env = new Environment(
                new TableRow(values.Select(list =>
                    list.IsNull || ((AttributeTypeCollection) list.AttributeType).ElementType ==
                    AttributeTypePrimitive.Null
                        ? ValueNull.Instance as Value
                        : list)), attributes, true);
            var result = new CondExprVisitor(_zmi, env).Visit(context.cond_expr());

            return result.Bind(res => alias == null
                ? new QueryResult(res.Value).Just()
                : new QueryResult(new Attribute(alias.GetText()), res.Value).Just());
        }
    }

    public class CondExprVisitor : QueryBaseVisitor<Maybe<Result>>
    {
        private readonly ZMI _zmi;
        private readonly Environment _env;

        public CondExprVisitor(ZMI zmi, Environment env)
        {
            _zmi = zmi;
            _env = env;
        }

        public override Maybe<Result> VisitCond_expr(QueryParser.Cond_exprContext context) =>
            context.and_expr().Select(Visit).Aggregate((a, b) => a.Zip(b).Bind(tuple =>
                tuple.Item1
                    .Or(tuple.Item2)
                    .Just()));

        public override Maybe<Result> VisitAnd_expr(QueryParser.And_exprContext context) =>
            context.not_expr().Select(Visit).Aggregate((a, b) => a.Zip(b).Bind(tuple =>
                tuple.Item1
                    .And(tuple.Item2)
                    .Just()));

        public override Maybe<Result> VisitNot_expr(QueryParser.Not_exprContext context) => context.NOT() != null
            ? Visit(context.not_expr()).Bind(n => n.Negate().Just())
            : Visit(context.bool_expr());

        public override Maybe<Result> VisitBool_expr(QueryParser.Bool_exprContext context)
        {
            var boolExprInt = new BoolExprVisitor(_zmi, _env);
            return boolExprInt.Visit(context);
        }
    }

    public class BoolExprVisitor : QueryBaseVisitor<Maybe<Result>>
    {
        private readonly ZMI _zmi;
        private readonly Environment _env;

        public BoolExprVisitor(ZMI zmi, Environment env)
        {
            _zmi = zmi;
            _env = env;
        }

        public override Maybe<Result> VisitBool_expr(QueryParser.Bool_exprContext context)
        {
            if (context.rel_op() != null)
                return VisitOp(context);
            if (context.REGEXP() != null)
                return VisitRegExp(context);
            return VisitBasic(context);
        }

        private Maybe<Result> VisitOp(QueryParser.Bool_exprContext context)
        {
            var left = new BasicExprVisitor(_zmi, _env).Visit(context.basic_expr()[0]);
            var right = new BasicExprVisitor(_zmi, _env).Visit(context.basic_expr()[1]);
            return left.Zip(right)
                .Bind(tuple => new RelOpVisitor((tuple.Item1, tuple.Item2)).Visit(context.rel_op()));
        }

        private Maybe<Result> VisitRegExp(QueryParser.Bool_exprContext context)
        {
            var left = new BasicExprVisitor(_zmi, _env).Visit(context.basic_expr()[0]);
            return left.Bind(l =>
                l.RegExpr(new ResultSingle(new ValueString(context.string_const().GetText().Trim('\"'))))
                    .Just());
        }

        private Maybe<Result> VisitBasic(QueryParser.Bool_exprContext context) =>
            new BasicExprVisitor(_zmi, _env).Visit(context.basic_expr()[0]);
    }

    public class BasicExprVisitor : QueryBaseVisitor<Maybe<Result>>
    {
        private readonly ZMI _zmi;
        private readonly Environment _env;

        public BasicExprVisitor(ZMI zmi, Environment env)
        {
            _zmi = zmi;
            _env = env;
        }

        public override Maybe<Result> VisitBasic_expr(QueryParser.Basic_exprContext context)
        {
            if (context.ADD() != null)
                return VisitAdd(context);
            if (context.SUB() != null)
                return VisitSub(context);
            return VisitFact_expr(context.fact_expr());
        }

        private (Maybe<Result>, Maybe<Result>) VisitTwo(QueryParser.Basic_exprContext context)
        {
            var left = new BasicExprVisitor(_zmi, _env).Visit(context.basic_expr());
            var right = new BasicExprVisitor(_zmi, _env).Visit(context.fact_expr());
            return (left, right);
        }

        private Maybe<Result> VisitAdd(QueryParser.Basic_exprContext context)
        {
            var (left, right) = VisitTwo(context);
            return left.Zip(right).Bind(tuple => tuple.Item1.Add(tuple.Item2).Just());
        }

        private Maybe<Result> VisitSub(QueryParser.Basic_exprContext context)
        {
            var (left, right) = VisitTwo(context);
            return left.Zip(right).Bind(tuple => tuple.Item1.Subtract(tuple.Item2).Just());
        }

        public override Maybe<Result> VisitFact_expr(QueryParser.Fact_exprContext context)
        {
            if (context.MUL() != null)
                return VisitMul(context);
            if (context.DIV() != null)
                return VisitDiv(context);
            if (context.MOD() != null)
                return VisitMod(context);
            return VisitNeg_expr(context.neg_expr());
        }

        private (Maybe<Result>, Maybe<Result>) VisitTwo(QueryParser.Fact_exprContext context)
        {
            var left = new BasicExprVisitor(_zmi, _env).Visit(context.fact_expr());
            var right = new BasicExprVisitor(_zmi, _env).Visit(context.neg_expr());
            return (left, right);
        }

        private Maybe<Result> VisitMul(QueryParser.Fact_exprContext context)
        {
            var (left, right) = VisitTwo(context);
            return left.Zip(right).Bind(tuple => tuple.Item1.Multiply(tuple.Item2).Just());
        }

        private Maybe<Result> VisitDiv(QueryParser.Fact_exprContext context)
        {
            var (left, right) = VisitTwo(context);
            return left.Zip(right).Bind(tuple => tuple.Item1.Divide(tuple.Item2).Just());
        }

        private Maybe<Result> VisitMod(QueryParser.Fact_exprContext context)
        {
            var (left, right) = VisitTwo(context);
            return left.Zip(right).Bind(tuple => tuple.Item1.Modulo(tuple.Item2).Just());
        }

        public override Maybe<Result> VisitNeg_expr(QueryParser.Neg_exprContext context)
        {
            if (context.SUB() != null)
                return VisitNeg_expr(context.neg_expr()).Bind(r => r.Negate().Just());
            return new TermExprVisitor(_zmi, _env).Visit(context.term_expr());
        }
    }

    public class TermExprVisitor : QueryBaseVisitor<Maybe<Result>>
    {
        private readonly ZMI _zmi;
        private readonly Environment _env;

        public TermExprVisitor(ZMI zmi, Environment env)
        {
            _zmi = zmi;
            _env = env;
        }

        public override Maybe<Result> VisitTerm_expr(QueryParser.Term_exprContext context)
        {
            if (context.string_const() != null)
                return new ResultSingle(new ValueString(context.string_const().GetText().Trim('\"'))).Just()
                    .FMap(Result.Id);
            if (context.bool_const() != null)
                return new ResultSingle(new ValueBoolean(context.bool_const().TRUE() != null)).Just()
                    .FMap(Result.Id);
            if (context.int_const() != null)
                return new ResultSingle(new ValueInt(int.Parse(context.int_const().GetText()))).Just()
                    .FMap(Result.Id);
            if (context.double_const() != null)
                return new ResultSingle(new ValueDouble(double.Parse(context.double_const().GetText(),
                    CultureInfo.InvariantCulture))).Just().FMap(Result.Id);
            if (context.LBRACE() != null)
                //return new ResultSingle(new ValueSet());
                throw new NotImplementedException("term_expr: {}");
            if (context.LBRACK() != null)
                throw new NotImplementedException("term_expr: []");
            if (context.LT() != null)
                throw new NotImplementedException("term_expr: <...>");

            if (context.cond_expr() != null)
                return new CondExprVisitor(_zmi, _env).Visit(context.cond_expr());
            if (context.statement() != null)
                return VisitStmt(context.statement()).FMap(Result.Id);

            var id = context.identifier().ID().GetText();
            if (context.LPAREN() != null)
                return VisitFunction(id, context);

            var cell = _env.GetIdent(id, out var hasColumn);
            return !hasColumn ? Maybe<Result>.Nothing : cell.Just();
        }

        private Maybe<Result> VisitFunction(string id, QueryParser.Term_exprContext context)
        {
            List<Result> arguments;
            if (context.MUL() != null)
                arguments = _env.ToList();
            else
            {
                var searchedFor = context.expr_list()?.cond_expr()
                    .Select(c => new CondExprVisitor(_zmi, _env).Visit(c))
                    .Sequence();
                if (searchedFor == null)
                    arguments = new List<Result>();
                else if (searchedFor.HasNothing)
                    return Maybe<Result>.Nothing;
                else
                    arguments = searchedFor.Val;
            }

            return Functions.Instance.Evaluate(id, arguments);
        }

        private Maybe<ResultSingle> VisitStmt(QueryParser.StatementContext context)
        {
            var results = new QueryVisitor(_zmi).Visit(context).Sequence();
            if (results.HasNothing)
                return Maybe<ResultSingle>.Nothing;

            if (results.Val.Count != 1)
                throw new ArgumentException("Nested queries must SELECT exactly one item.");

            return new ResultSingle(results.Val.First().Value).Just();
        }
    }

    public class RelOpVisitor : QueryBaseVisitor<Maybe<Result>>
    {
        private readonly (Result, Result) _pair;

        public RelOpVisitor((Result, Result) pair) => _pair = pair;

        public override Maybe<Result> VisitRel_op(QueryParser.Rel_opContext context)
        {
            var (left, right) = _pair;
            if (context.GT() != null)
                return left.IsLowerThan(right).Negate().And(left.IsEqual(right).Negate()).Just();
            return VisitRel_op_no_gt(context.rel_op_no_gt());
        }

        public override Maybe<Result> VisitRel_op_no_gt(QueryParser.Rel_op_no_gtContext context)
        {
            var (left, right) = _pair;
            var token = ((ITerminalNode) context.GetChild(0)).Symbol.Type;
            var res = token switch
            {
                QueryParser.EQ => left.IsEqual(right),
                QueryParser.NEQ => left.IsEqual(right).Negate(),
                QueryParser.LT => left.IsLowerThan(right),
                QueryParser.LE => left.IsLowerThan(right).Or(left.IsEqual(right)),
                QueryParser.GE => left.IsLowerThan(right).Negate(),
                _ => throw new InternalInterpreterException("No such token in RelOp")
            };
            return res.Just();
        }
    }
}