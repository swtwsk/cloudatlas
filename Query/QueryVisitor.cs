namespace CloudAtlas.Query
{
    public class QueryVisitor : QueryBaseVisitor<object>
    {
        public override object VisitProgram(QueryParser.ProgramContext context)
        {
            return base.VisitProgram(context);
        }

        public override object VisitStatement_list(QueryParser.Statement_listContext context)
        {
            return base.VisitStatement_list(context);
        }

        public override object VisitStatement(QueryParser.StatementContext context)
        {
            return base.VisitStatement(context);
        }

        public override object VisitWhere_clause(QueryParser.Where_clauseContext context)
        {
            return base.VisitWhere_clause(context);
        }

        public override object VisitOrder_by_clause(QueryParser.Order_by_clauseContext context)
        {
            return base.VisitOrder_by_clause(context);
        }

        public override object VisitOrder_list(QueryParser.Order_listContext context)
        {
            return base.VisitOrder_list(context);
        }

        public override object VisitOrder_item(QueryParser.Order_itemContext context)
        {
            return base.VisitOrder_item(context);
        }

        public override object VisitOrder(QueryParser.OrderContext context)
        {
            return base.VisitOrder(context);
        }

        public override object VisitNulls(QueryParser.NullsContext context)
        {
            return base.VisitNulls(context);
        }

        public override object VisitSel_list(QueryParser.Sel_listContext context)
        {
            return base.VisitSel_list(context);
        }

        public override object VisitSel_item(QueryParser.Sel_itemContext context)
        {
            return base.VisitSel_item(context);
        }

        public override object VisitSel_modifier(QueryParser.Sel_modifierContext context)
        {
            return base.VisitSel_modifier(context);
        }

        public override object VisitSel_expr(QueryParser.Sel_exprContext context)
        {
            return base.VisitSel_expr(context);
        }

        public override object VisitCond_expr(QueryParser.Cond_exprContext context)
        {
            return base.VisitCond_expr(context);
        }

        public override object VisitCond_expr_no_gt(QueryParser.Cond_expr_no_gtContext context)
        {
            return base.VisitCond_expr_no_gt(context);
        }

        public override object VisitAnd_expr(QueryParser.And_exprContext context)
        {
            return base.VisitAnd_expr(context);
        }

        public override object VisitAnd_expr_no_gt(QueryParser.And_expr_no_gtContext context)
        {
            return base.VisitAnd_expr_no_gt(context);
        }

        public override object VisitNot_expr(QueryParser.Not_exprContext context)
        {
            return base.VisitNot_expr(context);
        }

        public override object VisitNot_expr_no_gt(QueryParser.Not_expr_no_gtContext context)
        {
            return base.VisitNot_expr_no_gt(context);
        }

        public override object VisitBool_expr(QueryParser.Bool_exprContext context)
        {
            return base.VisitBool_expr(context);
        }

        public override object VisitBool_expr_no_gt(QueryParser.Bool_expr_no_gtContext context)
        {
            return base.VisitBool_expr_no_gt(context);
        }

        public override object VisitBasic_expr(QueryParser.Basic_exprContext context)
        {
            return base.VisitBasic_expr(context);
        }

        public override object VisitFact_expr(QueryParser.Fact_exprContext context)
        {
            return base.VisitFact_expr(context);
        }

        public override object VisitNeg_expr(QueryParser.Neg_exprContext context)
        {
            return base.VisitNeg_expr(context);
        }

        public override object VisitTerm_expr(QueryParser.Term_exprContext context)
        {
            return base.VisitTerm_expr(context);
        }

        public override object VisitIdentifier(QueryParser.IdentifierContext context)
        {
            return base.VisitIdentifier(context);
        }

        public override object VisitString_const(QueryParser.String_constContext context)
        {
            return base.VisitString_const(context);
        }

        public override object VisitBool_const(QueryParser.Bool_constContext context)
        {
            return base.VisitBool_const(context);
        }

        public override object VisitInt_const(QueryParser.Int_constContext context)
        {
            return base.VisitInt_const(context);
        }

        public override object VisitDouble_const(QueryParser.Double_constContext context)
        {
            return base.VisitDouble_const(context);
        }

        public override object VisitExpr_list(QueryParser.Expr_listContext context)
        {
            return base.VisitExpr_list(context);
        }

        public override object VisitExpr_list_no_gt(QueryParser.Expr_list_no_gtContext context)
        {
            return base.VisitExpr_list_no_gt(context);
        }

        public override object VisitRel_op(QueryParser.Rel_opContext context)
        {
            return base.VisitRel_op(context);
        }

        public override object VisitRel_op_no_gt(QueryParser.Rel_op_no_gtContext context)
        {
            return base.VisitRel_op_no_gt(context);
        }

        public override object VisitError(QueryParser.ErrorContext context)
        {
            return base.VisitError(context);
        }
    }
}