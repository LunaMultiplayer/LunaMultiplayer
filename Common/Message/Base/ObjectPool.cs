using System;

namespace LunaCommon.Message.Base
{
    /// <summary>
    /// A simple lightweight object pool for fast and simple object reuse.
    /// Fast lightweight thread-safe object pool for objects that are expensive to create or could efficiently be reused.
    /// Note: this nuget package contains c# source code and depends on System.Collections.Concurrent introduced in .Net 4.0.
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        static System.Collections.Concurrent.ConcurrentStack<T> m_bag = new System.Collections.Concurrent.ConcurrentStack<T>();

        /// <summary>
        /// Maximum capactity of the pool, used in Put to discards objects if the capacity was reached.
        /// </summary>
        public static int MaxCapacity = 10;

        /// <summary>
        /// Default instance factory method, used to create new instances if the pool is empty.
        /// </summary>
        public static Func<T> DefaultInstanceFactory = null;

        /// <summary>
        /// Default method to be called whenever ans instance should be disposed.<para/>
        /// Used when the MaxCapacity is reached or when the Clear method is called.
        /// </summary>
        public static Action<T> DefaultInstanceDispose = null;

        /// <summary>
        /// Removes a stored object from the pool and return it.
        /// If the pool is empty, instanceFactory will be called to generate a new object.
        /// </summary>
        /// <param name="instanceFactory">The instance factory method used to create a new instance if pool is empty.</param>
        public static T Get(Func<T> instanceFactory)
        {
            T item;
            if (!m_bag.TryPop(out item))
            {
                return instanceFactory();
            }
            return item;
        }

        /// <summary>
        /// Removes a stored object from the pool and return it.
        /// If the pool is empty and a 'DefaultInstanceFactory' was provided, 
        /// then 'DefaultInstanceFactory' will be called to generate a new object,
        /// otherwise null is returned.
        /// </summary>
        public static T Get()
        {
            T item;
            if (!m_bag.TryPop(out item))
            {
                if (DefaultInstanceFactory == null)
                    return null;
                return DefaultInstanceFactory();
            }
            return item;
        }

        /// <summary>
        /// Puts the specified item in the pool.
        /// Is the 'MaxCapacity' has been reached the item is ignored.
        /// If a Default Intance Dispose method was provided, it will be called for the ignored item.
        /// </summary>
        public static void Put(T item)
        {
            // add to pool if it is not full
            if (m_bag.Count < MaxCapacity)
            {
                m_bag.Push(item);
            }
            else if (DefaultInstanceDispose != null)
            {
                DefaultInstanceDispose(item);
            }
        }

        /// <summary>
        /// Clears this instance by removing all stored items.<para/>
        /// If a Default Intance Dispose method was provided, it will be called for
        /// every remove item.
        /// </summary>
        public static void Clear()
        {
            if (DefaultInstanceDispose != null)
            {
                T item;
                while (m_bag.TryPop(out item))
                {
                    DefaultInstanceDispose(item);
                }
            }
            m_bag.Clear();
        }

        /// <summary>
        /// Gets the number of objects in the pool.
        /// </summary>
        /// <value>The count.</value>
        public static int Count
        {
            get { return m_bag.Count; }
        }
    }
}
