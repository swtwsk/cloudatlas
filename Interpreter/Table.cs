using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CloudAtlas.Interpreter.Exceptions;
using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class Table : IEnumerable<TableRow>
    {
        private readonly List<string> _columns;
        private readonly Dictionary<string, int> _headersMap;
        private readonly List<TableRow> _rows = new List<TableRow>();

        public ImmutableList<string> Columns => _columns?.ToImmutableList();

        public Table(ZMI zmi)
        {
            _columns = zmi.Sons.SelectMany(z => z.Attributes).Select(e => e.Key.Name).Distinct().ToList();
            _headersMap = _columns.Select((s, i) => (s, i)).ToDictionary(tuple => tuple.s, tuple => tuple.i);

            foreach (var z in zmi.Sons)
            {
                var row = new Value[_columns.Count];
                for (var j = 0; j < row.Length; j++)
                    row[j] = ValueNull.Instance;
                foreach (var (key, value) in z.Attributes)
                    row[GetColumnIndex(key.Name)] = value;
                AppendRow(new TableRow(row));
            }
        }

        public Table(Table table)
        {
            _columns = new List<string>(table._columns);
            _headersMap = new Dictionary<string, int>(table._headersMap);
        }

        public ValueList GetColumn(string column)
        {
            if (column.StartsWith("&"))
                throw new NoSuchAttributeException(column);
            
            if (!_headersMap.TryGetValue(column, out var position))
                throw new NoSuchAttributeException(column);
            
            var result = _rows.Select(row => row[position]).ToList();
            var elementType = AttributeTypeCollection.ComputeElementType(result);
            return new ValueList(result, elementType);
        }

        public void Sort(IComparer<TableRow> comparer) => _rows.Sort(comparer);

        public void AppendRow(TableRow row)
        {
            if (row.Count != _columns.Count)
                throw new InternalInterpreterException(
                    $"Cannot append row. Length expected: {_columns.Count} , got: {row.Count}.");
            _rows.Add(row);
        }

        public int GetColumnIndex(string column)
        {
            if (!_headersMap.TryGetValue(column, out var result))
                throw new NoSuchAttributeException(column);
            return result;
        }

        public IEnumerator<TableRow> GetEnumerator() => _rows.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}