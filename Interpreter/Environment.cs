using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shared.Model;

namespace Interpreter
{
    public class Environment : IEnumerable<Result>
    {
        private readonly TableRow _row;
        private readonly Dictionary<string, int> _columns;
        private readonly bool _isColumn;

        public Environment(TableRow row, IEnumerable<string> columns, bool isColumn = false) {
            _row = row;
            _columns = columns.Select((s, i) => (s, i)).ToDictionary(t => t.s, t => t.i);
            _isColumn = isColumn;
        }

        public Result GetIdent(string ident, out bool hasColumn)
        {
            hasColumn = _columns.TryGetValue(ident, out var column);
            if (!hasColumn || !_row.TryGet(column, out var cell))
                return new ResultSingle(ValueNull.Instance);
            
            if (_isColumn && cell is ValueList cellList)  // TODO: check it
                return new ResultColumn(cellList);
            
            return new ResultSingle(cell);
        }

        public IEnumerator<Result> GetEnumerator() => _columns
            .Select(pair => pair.Value)
            .Select(i => _row.TryGet(i, out var cell) ? cell : null)
            .Where(value => value != null)
            .Select(v => new ResultSingle(v))
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
