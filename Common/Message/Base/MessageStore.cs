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
        internal static ConcurrentDictionary<Type, ConcurrentQueue<IMessageData>> MessageDataDictionary = new ConcurrentDictionary<Type, ConcurrentQueue<IMessageData>>();
        internal static ConcurrentDictionary<Type, ConcurrentQueue<IMessageBase>> MessageDictionary = new ConcurrentDictionary<Type, ConcurrentQueue<IMessageBase>>();

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageDataConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();

        internal static T GetMessageData<T>(bool setAsRecycled = false) where T : class, IMessageData
        {
            var msgDataQueue = MessageDataDictionary.GetOrAdd(typeof(T), new ConcurrentQueue<IMessageData>());
            if (msgDataQueue.TryPeek(out var messageData) && messageData.ReadyToRecycle)
            {
                msgDataQueue.TryDequeue(out messageData);
                //We found a messageData that is already used so set it as in use and return it
                messageData.ReadyToRecycle = false;
                msgDataQueue.Enqueue(messageData);

                return messageData as T;
            }

            var newMsgData = CreateNewInstance<T>();
            if (setAsRecycled)
                newMsgData.ReadyToRecycle = true;

            msgDataQueue.Enqueue(newMsgData);

            return newMsgData;
        }

        internal static IMessageData GetMessageData(Type messageDataType)
        {
            var msgDataQueue = MessageDataDictionary.GetOrAdd(messageDataType, new ConcurrentQueue<IMessageData>());
            if (msgDataQueue.TryPeek(out var messageData) && messageData.ReadyToRecycle)
            {
                msgDataQueue.TryDequeue(out messageData);
                //We found a messageData that is already used so set it as in use and return it
                messageData.ReadyToRecycle = false;
                msgDataQueue.Enqueue(messageData);

                return messageData;
            }

            var newMsgData = CreateNewMessageDataInstance(messageDataType);

            msgDataQueue.Enqueue(newMsgData);

            return newMsgData;
        }

        internal static T GetMessage<T>() where T : class, IMessageBase
        {
            var msgQueue = MessageDictionary.GetOrAdd(typeof(T), new ConcurrentQueue<IMessageBase>());
            if (msgQueue.TryPeek(out var message) && message.Data != null && message.Data.ReadyToRecycle)
            {
                //found a message that has been used so dequeue it and put it in the back
                msgQueue.TryDequeue(out message);
                msgQueue.Enqueue(message);
                //We don't set the data.ReadyToRecycle = false as this should be handled by the GetMessageData

                return message as T;
            }

            var newMsgData = CreateNewInstance<T>();
            msgQueue.Enqueue(newMsgData);

            return newMsgData;
        }

        internal static IMessageBase GetMessage(Type type)
        {
            var msgQueue = MessageDictionary.GetOrAdd(type, new ConcurrentQueue<IMessageBase>());
            if (msgQueue.TryPeek(out var message) && message.Data != null && message.Data.ReadyToRecycle)
            {
                //found a message that has been used so dequeue it and put it in the back
                msgQueue.TryDequeue(out message);
                msgQueue.Enqueue(message);
                //We don't set the data.ReadyToRecycle = false as this should be handled by the GetMessageData

                return message;
            }

            var newMsg = CreateNewMessageInstance(type);
            msgQueue.Enqueue(newMsg);

            return newMsg;
        }

        /// <summary>
        /// Use it for statistics if you want
        /// </summary>
        public static int GetMessageCount(Type type)
        {
            if (type == null)
                return MessageDictionary.SelectMany(v => v.Value).Count();
            return MessageDictionary.TryGetValue(type, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// Use it for statistics if you want
        /// </summary>
        public static int GetMessageDataCount(Type type)
        {
            if (type == null)
                return MessageDataDictionary.SelectMany(v => v.Value).Count();
            return MessageDataDictionary.TryGetValue(type, out var list) ? list.Count : 0;
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
