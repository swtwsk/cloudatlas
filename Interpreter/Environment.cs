using System.Collections.Generic;
using System.Linq;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class Environment
    {
        private readonly TableRow _row;
        private readonly Dictionary<string, int> _columns;
        private readonly bool _isColumn;

        public Environment(TableRow row, IEnumerable<string> columns, bool isColumn = false) {
            _row = row;
            _columns = columns.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i);
            _isColumn = isColumn;
        }

        public Result GetIdent(string ident)
        {
            if (!_columns.TryGetValue(ident, out var column) || !_row.TryGet(column, out var cell))
                return new ResultSingle(ValueNull.Instance);
            
            if (_isColumn && cell is ValueList cellList)  // TODO: check it
                return new ResultColumn(cellList);
            
            return new ResultSingle(cell);
        }

        public Result this[string ident] => GetIdent(ident);
    }
}
