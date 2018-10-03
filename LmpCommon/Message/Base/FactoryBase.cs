using Lidgren.Network;
using LmpCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LmpCommon.Message.Base
{
    public abstract class FactoryBase
    {
        /// <summary>
        /// This dictionary contain all the messages that this factory handle
        /// </summary>
        private readonly Dictionary<uint, Type> _messageDictionary = new Dictionary<uint, Type>();

        /// <summary>
        /// In the constructor we run through this instance and get all the message that inherit BaseMsgType and add them to the dictionary
        /// </summary>
        protected FactoryBase()
        {
            var msgTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == BaseMsgType).ToArray();

            foreach (var msgType in msgTypes)
            {
                var constructor = msgType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (constructor == null)
                {
                    throw new Exception($"Message type {msgType.FullName} must have an internal parameter-less constructor");
                }

                var instance = constructor.Invoke(null);
                var typeProp = msgType.GetProperty("MessageType", BindingFlags.Public | BindingFlags.Instance);
                if (typeProp == null)
                {
                    throw new Exception($"Message type {msgType.FullName} must implement the MessageType property (uint)");
                }

                var typeVal = typeProp.GetValue(instance, null);
                _messageDictionary.Add((uint)(int)typeVal, msgType);
            }
        }

        /// <summary>
        /// Specify here the base type of the messages that this factory handle
        /// </summary>
        protected internal abstract Type BaseMsgType { get; }

        /// <summary>
        /// Call this method to deserialize a message
        /// </summary>
        /// <param name="lidgrenMsg">Lidgren message</param>
        /// <param name="receiveTime">Lidgren msg receive time property or LunaTime.Now</param>
        /// <returns>Full message with it's data filled</returns>
        public IMessageBase Deserialize(NetIncomingMessage lidgrenMsg, long receiveTime)
        {
            if (lidgrenMsg.LengthBytes >= 0)
            {
                var messageType = lidgrenMsg.ReadUInt16();
                var subtype = lidgrenMsg.ReadUInt16();
                lidgrenMsg.SkipPadBits();

                var msg = GetMessageByType(messageType);
                var data = msg.GetMessageData(subtype);

                data.Deserialize(lidgrenMsg);

                msg.SetData(data);
                msg.Data.ReceiveTime = receiveTime;
                msg.VersionMismatch = !LmpVersioning.IsCompatible(msg.Data.MajorVersion, msg.Data.MinorVersion, msg.Data.BuildVersion);

                return msg;
            }
            throw new Exception("Incorrect message length");
        }

        /// <summary>
        /// Specify if this factory handle client or server messages
        /// </summary>
        protected internal abstract Type HandledMessageTypes { get; }

        /// <summary>
        /// Method to retrieve a new message
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <typeparam name="TD">Message data type</typeparam>
        /// <returns>New empty message</returns>
        public T CreateNew<T, TD>() where T : class, IMessageBase
            where TD : class, IMessageData
        {
            var msg = MessageStore.GetMessage<T>();
            var data = MessageStore.GetMessageData<TD>();
            msg.SetData(data);

            return msg;
        }

        /// <summary>
        /// Method to retrieve a new message
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="data">Message data implementation</param>
        /// <returns>New message with the specified data</returns>
        public T CreateNew<T>(IMessageData data) where T : class, IMessageBase
        {
            var msg = MessageStore.GetMessage<T>();
            msg.SetData(data);
            return msg;
        }

        /// <summary>
        /// Method to retrieve a new message data
        /// </summary>
        /// <typeparam name="T">Message data type</typeparam>
        /// <returns>New empty message data</returns>
        public T CreateNewMessageData<T>() where T : class, IMessageData
        {
            return MessageStore.GetMessageData<T>();
        }

        /// <summary>
        /// Retrieves a message from the pool based on the type
        /// </summary>
        /// <param name="messageType">Message type</param>
        private IMessageBase GetMessageByType(ushort messageType)
        {
            if (Enum.IsDefined(HandledMessageTypes, (int)messageType) && _messageDictionary.ContainsKey(messageType))
            {
                var msg = MessageStore.GetMessage(_messageDictionary[messageType]);
                return msg;
            }
            throw new Exception("Cannot deserialize this type of message!");
        }
    }
}
