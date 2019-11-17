using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Shared.Model;
using Shared.Monads;
using Interpreter.Query;

namespace Interpreter
{
    public static class Interpreter
    {
        public static void ExecuteQueries(ZMI zmi, string query, bool log = false)
        {
            if (!zmi.Sons.Any()) 
                return;
			
            foreach (var son in zmi.Sons)
                ExecuteQueries(son, query, log);
			
            var lexer = new QueryLexer(new AntlrInputStream(query));
            var parser = new QueryParser(new CommonTokenStream(lexer));
            var result = parser.program().VisitProgram(zmi);
            var zone = zmi.PathName;
			
            foreach (var r in result) {
                if (log)
                    Console.WriteLine(zone + ": " + r);
                zmi.Attributes.AddOrChange(r.Name, r.Value);
            }
        }
        
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
