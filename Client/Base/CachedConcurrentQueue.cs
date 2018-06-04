using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Base
{
    public abstract class CachedConcurrentQueue<T, TD> where T : new() where TD : IMessageData
    {
        protected readonly ConcurrentBag<T> _cache = new ConcurrentBag<T>();
        protected ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public int CacheSize => _cache.Count;
        public int Count => _queue.Count;
        public bool IsEmpty => _queue.IsEmpty;

        public virtual void Enqueue(TD msgData)
        {
            if (_cache.TryTake(out var cachedValue))
            {
                AssignFromMessage(cachedValue, msgData);
                _queue.Enqueue(cachedValue);
            }
            else
            {
                var newVal = new T();
                AssignFromMessage(newVal, msgData);
                _queue.Enqueue(newVal);
            }
        }

        public virtual bool TryDequeue(out T result)
        {
            return _queue.TryDequeue(out result);
        }

        public virtual bool TryPeek(out T result)
        {
            return _queue.TryPeek(out result);
        }

        public virtual void Clear()
        {
            while (!_queue.IsEmpty && _queue.TryDequeue(out _)) { }
        }

        public virtual void Recycle(T item)
        {
            if (item != null) _cache.Add(item);
        }

        protected abstract void AssignFromMessage(T value, TD msgData);
    }
}
