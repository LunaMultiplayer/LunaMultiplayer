using Lidgren.Network;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Base
{
    public abstract class FactoryBase
    {
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
                var dataCompressed = lidgrenMsg.ReadBoolean();

                var msg = GetMessageByType(messageType);
                var data = msg.GetMessageData(subtype);

                data.Deserialize(lidgrenMsg, dataCompressed);

                msg.SetData(data);
                msg.Data.ReceiveTime = receiveTime;

                return msg;
            }
            throw new Exception("Incorrect message length");
        }

        /// <summary>
        /// Specify if this factory handle client or server messages
        /// </summary>
        protected abstract Type HandledMessageTypes { get; }

        /// <summary>
        ///     Include here all the client/server messages so they can be handled
        /// </summary>
        protected Dictionary<uint, Type> MessageDictionary { get; } = new Dictionary<uint, Type>();

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
            return MessageStore.GetMessageData<T>(); ;
        }
        
        /// <summary>
        /// Retrieves a message from the pool based on the type
        /// </summary>
        /// <param name="messageType">Message type</param>
        private IMessageBase GetMessageByType(ushort messageType)
        {
            if (Enum.IsDefined(HandledMessageTypes, (int)messageType) && MessageDictionary.ContainsKey(messageType))
            {
                var msg = MessageStore.GetMessage(MessageDictionary[messageType]);
                return msg;
            }
            throw new Exception("Cannot deserialize this type of message!");
        }
    }
}