using System.Collections.Concurrent;

namespace LMP.Server.Utilities
{
    public static class ExtensionMethods
    {
        public static ConcurrentQueue<T> CloneConcurrentQueue<T>(this ConcurrentQueue<T> queue)
        {
            var messages = new T[queue.Count];
            queue.CopyTo(messages, 0);

            return new ConcurrentQueue<T>(messages);
        }
    }
}
