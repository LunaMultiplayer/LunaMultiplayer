using System.Collections.Generic;

namespace LunaClient.Utilities
{
    public class BlockingQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly object _queueLock;

        public BlockingQueue()
        {
            _queue = new Queue<T>();
            _queueLock = new object();
        }

        public int Count()
        {
            lock (_queueLock)
            {
                return _queue.Count;
            }
        }

        public void Clear()
        {
            lock (_queueLock)
            {
                _queue.Clear();
            }
        }

        public void Enqueue(T data)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(data);
            }
        }

        public bool TryDequeue(out T data)
        {
            lock (_queueLock)
            {
                data = default(T);
                var success = false;
                if (_queue.Count > 0)
                {
                    data = _queue.Dequeue();
                    success = true;
                }
                return success;
            }
        }

        public bool TryPeek(out T data)
        {
            lock (_queueLock)
            {
                data = default(T);
                var success = false;
                if (_queue.Count > 0)
                {
                    data = _queue.Peek();
                    success = true;
                }
                return success;
            }
        }
    }
}