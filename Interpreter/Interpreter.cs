using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Shared.Model;
using Shared.Monads;
using Interpreter.Query;
using Shared.Logger;

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

            QueryParser.ProgramContext programContext = null;
            
            try
            {
                programContext = Parse(query);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            
            var result = programContext.VisitProgram(zmi);
            var zone = zmi.PathName;
			
            foreach (var r in result) {
                if (log)
                    Console.WriteLine(zone + ": " + r);
                zmi.Attributes.AddOrChange(r.Name, r.Value);
            }
        }

        public static QueryParser.ProgramContext Parse(string query)
        {
            var lexer = new QueryLexer(new AntlrInputStream(query));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowingErrorListener<int>());

            var parser = new QueryParser(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowingErrorListener<IToken>());

            return parser.program();
        }

        private static IEnumerable<QueryResult> VisitProgram(this QueryParser.ProgramContext context, ZMI zmi) =>
            VisitProgram(zmi, context);

        private static IEnumerable<QueryResult> VisitProgram(ZMI zmi, QueryParser.ProgramContext context)
        {
            var toReturn = new QueryVisitor(zmi).Visit(context).ToList();
            if (toReturn.Any(maybe => maybe.Match(v => v.Name == null, () => false)))
                throw new ArgumentException("All items in top-level SELECT must be aliased");
            return toReturn.Sequence().Match(list => list, () => new List<QueryResult>());
        }
    }
    
    // got from Antlr4 C# tutorial
    // https://github.com/michael-jay/antlr4-dotnet-core/blob/master/visitor/src/Calculator/Parsing/ThrowingErrorListener.cs
    internal class ThrowingErrorListener<TSymbol> : IAntlrErrorListener<TSymbol>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, TSymbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new Exception($"line {line}:{charPositionInLine} {msg}");
        }
    }
}
