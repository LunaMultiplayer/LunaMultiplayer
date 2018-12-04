using System.Collections.Generic;
using System.Linq;

namespace LmpCommon.Collection
{
    public class ConcurrentHashSet<T>
    {
        private readonly object _lock = new object();
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public bool Add(T item)
        {
            lock (_lock)
            {
                return _hashSet.Add(item);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _hashSet.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_lock)
            {
                return _hashSet.Contains(item);
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _hashSet.Remove(item);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _hashSet.Count;
                }
            }
        }

        public T[] GetValues
        {
            get
            {
                lock (_lock)
                {
                    return _hashSet.ToArray();
                }
            }
        }
    }
}
