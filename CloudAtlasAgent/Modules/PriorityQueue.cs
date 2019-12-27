using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CloudAtlasAgent.Modules
{
    // TODO: Implement proper PriorityQueue
    public class PriorityQueue<T> : IProducerConsumerCollection<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private List<T> _queue = new List<T>();
        private object _lock = new object();

        public void Enqueue(T elem)
        {
            lock (_lock)
            {
                _queue.Add(elem);
                _queue.Sort();
            }
        }

        public bool TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            return TryDequeue(out item);
        }

        public bool TryDequeue(out T elem)
        {
            elem = default;
            lock (_lock)
            {
                if (_queue.Count < 1)
                    return false;
                elem = _queue[0];
                _queue.RemoveAt(0);
            }

            return true;
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

        public int Count => _queue.Count;
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
