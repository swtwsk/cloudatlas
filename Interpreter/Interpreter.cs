using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Model;
using Shared.Monads;
using Interpreter.Query;

namespace Interpreter
{
    public static class Interpreter
    {
        public static List<QueryResult> VisitProgram(this QueryParser.ProgramContext context, ZMI zmi) =>
            VisitProgram(zmi, context);

        public static List<QueryResult> VisitProgram(ZMI zmi, QueryParser.ProgramContext context)
        {
            var toReturn = new QueryVisitor(zmi).Visit(context).ToList();
            if (toReturn.Any(maybe => maybe.Match(v => v.Name == null, () => false)))
                throw new ArgumentException("All items in top-level SELECT must be aliased");
            return toReturn.Sequence().Match(list => list, () => new List<QueryResult>());
        }
    }
}
