using LmpCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace LmpCommon.Message.Base
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
            if (!MessageDataDictionary.TryGetValue(message.Data.ClassName, out var dataBag))
            {
                dataBag = new ConcurrentBag<IMessageData>();
                MessageDataDictionary.TryAdd(message.Data.ClassName, dataBag);
            }
            dataBag.Add(message.Data);

            message.SetData(null);

            if (!MessageDictionary.TryGetValue(message.ClassName, out var messageBag))
            {
                messageBag = new ConcurrentBag<IMessageBase>();
                MessageDictionary.TryAdd(message.ClassName, messageBag);
            }
            messageBag.Add(message);
        }

        internal static T GetMessageData<T>() where T : class, IMessageData
        {
            if (MessageDataDictionary.TryGetValue(typeof(T).Name, out var bag) && bag.TryTake(out var messageData))
            {
                //We found a messageData that is already used so return it
                return messageData as T;
            }

            return CreateNewInstance<T>();
        }

        internal static IMessageData GetMessageData(Type messageDataType)
        {
            if (MessageDataDictionary.TryGetValue(messageDataType.Name, out var bag) && bag.TryTake(out var messageData))
            {
                return messageData;
            }

            return CreateNewMessageDataInstance(messageDataType);
        }

        internal static T GetMessage<T>() where T : class, IMessageBase
        {
            if (MessageDictionary.TryGetValue(typeof(T).Name, out var bag) && bag.TryTake(out var message))
            {
                //We found a messageData that is already used so return it
                message.SetData(null);
                return message as T;
            }

            return CreateNewInstance<T>();
        }

        internal static IMessageBase GetMessage(Type type)
        {
            if (MessageDictionary.TryGetValue(type.Name, out var bag) && bag.TryTake(out var message))
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
                if (!MessageDataConstructorDictionary.TryGetValue(typeof(T), out var ctor))
                {
                    ctor = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                    MessageDataConstructorDictionary.TryAdd(typeof(T), ctor);
                }

                return ctor.Invoke(null) as T;
            }
            if (typeof(IMessageBase).IsAssignableFrom(typeof(T)))
            {
                if (!MessageConstructorDictionary.TryGetValue(typeof(T), out var ctor))
                {
                    ctor = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                    MessageConstructorDictionary.TryAdd(typeof(T), ctor);
                }

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
                if (!MessageConstructorDictionary.TryGetValue(type, out var ctor))
                {
                    ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                    MessageConstructorDictionary.TryAdd(type, ctor);
                }

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
                if (!MessageDataConstructorDictionary.TryGetValue(type, out var ctor))
                {
                    ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
                    MessageDataConstructorDictionary.TryAdd(type, ctor);
                }

                return ctor.Invoke(null) as IMessageData;
            }

            throw new Exception("Cannot implement this object!");
        }
    }
}
