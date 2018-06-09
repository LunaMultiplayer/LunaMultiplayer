using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Base
{
    public abstract class CachedConcurrentQueue<T, TD> where T : new() where TD : IMessageData
    {
        protected static readonly ConcurrentBag<T> Cache = new ConcurrentBag<T>();
        protected ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();

        public static int CacheSize => Cache.Count;
        public int Count => Queue.Count;
        public bool IsEmpty => Queue.IsEmpty;

        public virtual void Enqueue(TD msgData)
        {
            if (Cache.TryTake(out var cachedValue))
            {
                AssignFromMessage(cachedValue, msgData);
                Queue.Enqueue(cachedValue);
            }
            else
            {
                var newVal = new T();
                AssignFromMessage(newVal, msgData);
                Queue.Enqueue(newVal);
            }
        }

        public virtual bool TryDequeue(out T result)
        {
            return Queue.TryDequeue(out result);
        }

        public virtual bool TryPeek(out T result)
        {
            return Queue.TryPeek(out result);
        }

        public virtual void Clear()
        {
            while (!Queue.IsEmpty && Queue.TryDequeue(out _)) { }
        }

        public virtual void Recycle(T item)
        {
            if (item != null) Cache.Add(item);
        }

        protected abstract void AssignFromMessage(T value, TD msgData);
    }
}
