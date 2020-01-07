using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CloudAtlasAgent.Modules
{
    public class PriorityQueue<T> : ICollection<T>, ICollection
        where T : IComparable<T>, IEquatable<T>
    {
        private Node _head;
        
        public void Add(T item)
        {
            var b = new Node(item);
            _head = Merge(_head, b);
        }

        public void Clear() => _head = null;

        public bool Contains(T item) => _head != null && _head.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // check array index valid index into array
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Array index is less than 0");
            }
            
            if (_head == null)
                return;

            var newArray = _head.ToEnumerable().ToArray();
            Array.Copy(newArray, 0, array, arrayIndex, newArray.Length);
        }

        public bool Remove(T item)
        {
            if (_head == null)
                return false;

            if (_head.Key.Equals(item))
            {
                _head = Merge(_head.Left, _head.Right);
                return true;
            }

            return RemoveRecursive(item, _head, out _head);
        }

        private bool RemoveRecursive(T item, Node node, out Node newNode)
        {
            if (node.Key.Equals(item))
            {
                newNode = Merge(node.Left, node.Right);
                return true;
            }

            newNode = node;

            if (node.Left != null && RemoveRecursive(item, node.Left, out var newLeft))
            {
                node.Left = newLeft;
                return true;
            }

            if (node.Right != null && RemoveRecursive(item, node.Right, out var newRight))
            {
                node.Right = newRight;
                return true;
            }

            return false;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // check array index valid index into array
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Array index is less than 0");
            }
            
            if (_head == null)
                return;

            var newArray = _head.ToEnumerable().ToArray();
            Array.Copy(newArray, 0, array, index, newArray.Length);
        }

        public int Count => _head?.Count() ?? 0;
        public bool IsSynchronized => false;
        public object SyncRoot => throw new NotImplementedException();
        public bool IsReadOnly => false;

        public T Pop()
        {
            var x = _head.Key;
            _head = Merge(_head.Left, _head.Right);
            return x;
        }

        public bool TryPop(out T item)
        {
            item = default;
            if (_head == null)
                return false;
            item = Pop();
            return true;
        }
        
        private class Node
        {
            public Node(T key)
            {
                Key = key;
                Left = null;
                Right = null;
            }

            public T Key { get; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public int SVal { get; set; }

            public bool Contains(T item)
            {
                if (Key.Equals(item))
                    return true;

                return Left != null && Left.Contains(item) || Right != null && Right.Contains(item);
            }

            public int Count()
            {
                var sum = 1;
                if (Left != null)
                    sum += Left.Count();
                if (Right != null)
                    sum += Right.Count();
                return sum;
            }

            public IEnumerable<T> ToEnumerable()
            {
                IEnumerable<T> enumerable = null;
                
                if (Left != null)
                    enumerable = Left.ToEnumerable();
                enumerable = enumerable == null ? new[] {Key} : enumerable.Concat(new[] {Key});
                if (Right != null)
                    enumerable = enumerable.Concat(Right.ToEnumerable());

                return enumerable;
            }
        }

        private Node Merge(Node a, Node b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;
            if (a.Key.CompareTo(b.Key) > 0)
            {
                var temp = a;
                a = b;
                b = temp;
            }

            a.Right = Merge(a.Right, b);

            if (a.Left == null)
            {
                a.Left = a.Right;
                a.Right = null;
            }
            else
            {
                if (a.Left.SVal < a.Right.SVal)
                {
                    var temp = a.Left;
                    a.Left = a.Right;
                    a.Right = temp;
                }

                a.SVal = a.Right.SVal + 1;
            }

            return a;
        }

        public IEnumerator<T> GetEnumerator() => _head.ToEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public class BlockingPriorityQueue<T> : IProducerConsumerCollection<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private readonly PriorityQueue<T> _queue = new PriorityQueue<T>();
        private readonly object _lock = new object();

        public bool TryAdd(T item)
        {
            lock (_lock)
                _queue.Add(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            item = default;
            bool accomplished;
            lock (_lock)
                accomplished = _queue.TryPop(out item);
            return accomplished;
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<T> queueCopy;
            lock (_lock)
                queueCopy = new List<T>(_queue);
            return queueCopy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            lock (_lock)
                ((ICollection) _queue).CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public bool IsSynchronized => true;
        public object SyncRoot => _lock;

        public void CopyTo(T[] array, int index)
        {
            lock (_lock)
                _queue.CopyTo(array, index);
        }

        public T[] ToArray()
        {
            T[] rval;
            lock (_lock)
                rval = _queue.ToArray();
            return rval;
        }
    }
}
