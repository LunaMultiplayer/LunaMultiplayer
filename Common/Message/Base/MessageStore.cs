using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        internal static ConcurrentDictionary<Type, List<IMessageData>> MessageDataDictionary = new ConcurrentDictionary<Type, List<IMessageData>>();
        internal static ConcurrentDictionary<Type, List<IMessageBase>> MessageDictionary = new ConcurrentDictionary<Type, List<IMessageBase>>();

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageDataConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> MessageConstructorDictionary = new ConcurrentDictionary<Type, ConstructorInfo>();

        internal static T GetMessageData<T>(bool setAsRecycled = false) where T : class, IMessageData
        {
            var msgDataDictionary = MessageDataDictionary.GetOrAdd(typeof(T), new List<IMessageData>());
            var msgData = msgDataDictionary.FirstOrDefault(m => m.ReadyToRecycle);
            if (msgData != null)
            {
                //We found a messageData that is already used so set it as in use and return it
                msgData.ReadyToRecycle = false;
                return msgData as T;
            }

            var newMsgData = CreateNewInstance<T>();
            if (setAsRecycled)
                newMsgData.ReadyToRecycle = true;

            msgDataDictionary.Add(newMsgData);

            return newMsgData;
        }

        internal static T GetMessage<T>() where T : class, IMessageBase
        {
            var msgDictionary = MessageDictionary.GetOrAdd(typeof(T), new List<IMessageBase>());
            var msg = msgDictionary.FirstOrDefault(m => m.Data != null && m.Data.ReadyToRecycle);
            if (msg != null)
            {
                //We found a messageData that is already used so return it.
                //We don't set the data.ReadyToRecycle = false as this should be handled by the GetMessageData
                return msg as T;
            }

            var newMsgData = CreateNewInstance<T>();
            msgDictionary.Add(newMsgData);

            return newMsgData;
        }

        internal static IMessageBase GetMessage(Type type)
        {
            var msgDictionary = MessageDictionary.GetOrAdd(type, new List<IMessageBase>());
            var msg = msgDictionary.FirstOrDefault(m => m.Data != null && m.Data.ReadyToRecycle);
            if (msg != null)
            {
                //We found a messageData that is already used so return it.
                //We don't set the data.ReadyToRecycle = false as this should be handled by the GetMessageData
                return msg;
            }

            var newMsgData = CreateNewInstance(type);
            msgDictionary.Add(newMsgData);

            return newMsgData;
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
                var type = typeof(T);
                var ctorI = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

                var ctor = MessageConstructorDictionary.GetOrAdd(typeof(T), typeof(T)
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as T;
            }

            throw new Exception("Cannot implement this object!");
        }

        /// <summary>
        /// This method is much faster than Activator.CreateInstance and also that woudn't work as constructors are internal
        /// </summary>
        private static IMessageBase CreateNewInstance(Type type)
        {
            if (typeof(IMessageBase).IsAssignableFrom(type))
            {
                var ctor = MessageConstructorDictionary.GetOrAdd(type, type
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First());

                return ctor.Invoke(null) as IMessageBase;
            }

            throw new Exception("Cannot implement this object!");
        }
    }
}
