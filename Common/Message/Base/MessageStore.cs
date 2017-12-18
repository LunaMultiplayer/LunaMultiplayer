using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace LunaCommon.Message.Base
{
    /// <summary>
    /// This class is intended to add a cache layer over message and messagedata.
    /// We don't want to do a "new()" over messages and messagedata as the unity GC will be triggered very often.
    /// </summary>
    public static class MessageStore
    {
        internal static ConcurrentDictionary<string, ConcurrentBag<IMessageData>> MessageDataDictionary = new ConcurrentDictionary<string, ConcurrentBag<IMessageData>>();
        internal static ConcurrentDictionary<string, ConcurrentBag<IMessageBase>> MessageDictionary = new ConcurrentDictionary<string, ConcurrentBag<IMessageBase>>();

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageDataConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();

        internal static void RecycleMessage(IMessageBase message)
        {
            var msgDataQueue = MessageDataDictionary.GetOrAdd(message.Data.ClassName, new ConcurrentBag<IMessageData>());
            msgDataQueue.Add(message.Data);
            message.SetData(null);

            var msgQueue = MessageDictionary.GetOrAdd(message.ClassName, new ConcurrentBag<IMessageBase>());
            msgQueue.Add(message);
        }

        internal static T GetMessageData<T>() where T : class, IMessageData
        {
            var msgDataQueue = MessageDataDictionary.GetOrAdd(typeof(T).Name, new ConcurrentBag<IMessageData>());
            if (msgDataQueue.TryTake(out var messageData))
            {
                //We found a messageData that is already used so return it
                return messageData as T;
            }
            
            return CreateNewInstance<T>();
        }

        internal static IMessageData GetMessageData(Type messageDataType)
        {
            var msgDataQueue = MessageDataDictionary.GetOrAdd(messageDataType.Name, new ConcurrentBag<IMessageData>());
            if (msgDataQueue.TryTake(out var messageData))
            {
                return messageData;
            }
            
            return CreateNewMessageDataInstance(messageDataType);
        }

        internal static T GetMessage<T>() where T : class, IMessageBase
        {
            var msgQueue = MessageDictionary.GetOrAdd(typeof(T).Name, new ConcurrentBag<IMessageBase>());
            if (msgQueue.TryTake(out var message))
            {
                //We found a messageData that is already used so return it
                message.SetData(null);
                return message as T;
            }
            
            return CreateNewInstance<T>();
        }

        internal static IMessageBase GetMessage(Type type)
        {
            var msgQueue = MessageDictionary.GetOrAdd(type.Name, new ConcurrentBag<IMessageBase>());
            if (msgQueue.TryTake(out var message))
            {
                //We found a messageData that is already used so return it
                return message;
            }
            
            return CreateNewMessageInstance(type);
        }

        /// <summary>
        /// Use it for statistics if you want
        /// </summary>
        public static int GetMessageCount(Type type)
        {
            if (type == null)
                return MessageDictionary.SelectMany(v => v.Value).Count();
            return MessageDictionary.TryGetValue(type.Name, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// Use it for statistics if you want
        /// </summary>
        public static int GetMessageDataCount(Type type)
        {
            if (type == null)
                return MessageDataDictionary.SelectMany(v => v.Value).Count();
            return MessageDataDictionary.TryGetValue(type.Name, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// This method is much faster than Activator.CreateInstance and also that woudn't work as constructors are internal
        /// </summary>
        private static T CreateNewInstance<T>() where T : class
        {
            if (typeof(IMessageData).IsAssignableFrom(typeof(T)))
            {
                var ctor = MessageDataConstructorDictionary.GetOrAdd(typeof(T), typeof(T)
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as T;
            }
            if (typeof(IMessageBase).IsAssignableFrom(typeof(T)))
            {
                var ctor = MessageConstructorDictionary.GetOrAdd(typeof(T), typeof(T)
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as T;
            }

            throw new Exception("Cannot implement this object!");
        }

        /// <summary>
        /// This method is much faster than Activator.CreateInstance and also that woudn't work as constructors are internal
        /// </summary>
        private static IMessageBase CreateNewMessageInstance(Type type)
        {
            if (typeof(IMessageBase).IsAssignableFrom(type))
            {
                var ctor = MessageConstructorDictionary.GetOrAdd(type, type
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as IMessageBase;
            }

            throw new Exception("Cannot implement this object!");
        }

        /// <summary>
        /// This method is much faster than Activator.CreateInstance and also that woudn't work as constructors are internal
        /// </summary>
        private static IMessageData CreateNewMessageDataInstance(Type type)
        {
            if (typeof(IMessageData).IsAssignableFrom(type))
            {
                var ctor = MessageDataConstructorDictionary.GetOrAdd(type, type
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as IMessageData;
            }

            throw new Exception("Cannot implement this object!");
        }
    }
}
