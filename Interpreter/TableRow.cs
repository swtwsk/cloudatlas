using System.Collections.Generic;
using System.Linq;
using Shared.Model;

namespace Interpreter
{
    // TODO: Ensure immutable
    public class TableRow
    {
        private readonly Value[] _columns;
        
        public TableRow(IEnumerable<Value> values)
        {
            _columns = values.ToArray();
        }

        public int Size => _columns.Length;
        public int Count => Size;

        public bool TryGet(int i, out Value column)
        {
            column = null;
            if (_columns.Length <= i)
                return false;
            
            column = _columns[i];
            return true;
        }
        
        public Value this[int i] => _columns[i];
    }
}