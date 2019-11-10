using System.Collections.Generic;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class Environment
    {
        private readonly TableRow _row;
        private readonly Dictionary<string, int> _columns;

        public Environment(TableRow row, IEnumerable<string> columns) {
            _row = row;
            _columns = columns.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i);
        }

        public Result GetIdent(string ident)
        {
            return !_columns.TryGetValue(ident, out var column) || !_row.TryGet(column, out var cell)
                ? new ResultSingle(ValueNull.Instance)
                : new ResultSingle(cell);
        }

        public Result this[string ident] => GetIdent(ident);
    }
}
