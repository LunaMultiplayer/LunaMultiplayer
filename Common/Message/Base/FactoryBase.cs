using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaCommon.Message.Base
{
    public abstract class FactoryBase
    {
        private readonly bool _compress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compress">Compress the messages or not</param>
        protected FactoryBase(bool compress)
        {
            _compress = compress;
        }

        /// <summary>
        /// Call this method to deserialize a message
        /// </summary>
        /// <param name="data">Array of bytes with data</param>
        /// <param name="receiveTime">Lidgren msg receive time property or Datetime.Now</param>
        /// <returns>Full message with it's data filled</returns>
        public IMessageBase Deserialize(byte[] data, long receiveTime)
        {
            try
            {
                if (data.Length >= MessageConstants.HeaderLength)
                {
                    var messageType = DeserializeMessageType(data);
                    var subtype = DeserializeMessageSubType(data);
                    var length = DeserializeMessageLength(data);
                    var dataCompressed = DeserializeMessageCompressed(data);

                    var contentWithoutHeader = data.Skip(MessageConstants.HeaderLength).ToArray();

                    if (contentWithoutHeader.Length == length)
                    {
                        var msg = GetMessageByType(messageType, subtype, contentWithoutHeader, dataCompressed);
                        msg.Data.ReceiveTime = receiveTime;
                        return msg;
                    }
                    throw new Exception("Message data size mismatch");
                }
                throw new Exception("Message length below header size");
            }
            catch (Exception)
            {
                //Bad message, we couldn't deserialize
                //TODO: Properly log this error
                //LunaLog.Error($"Error deserializing message: {e}");
                return null;
            }
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
        public T CreateNew<T,TD>() where T : class, IMessageBase
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
        /// Retrieves the message type
        /// </summary>
        /// <param name="data">Full message data</param>
        /// <returns>Message type to be parsed as an enum</returns>
        private static ushort DeserializeMessageType(IEnumerable<byte> data)
        {
            var typeArray = data.Take(MessageConstants.MessageTypeLength).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(typeArray);
            var type = BitConverter.ToUInt16(typeArray, 0);
            return type;
        }

        /// <summary>
        /// Retrieves the message sub-type
        /// </summary>
        /// <param name="data">Full message data</param>
        /// <returns>Message sub-type to be parsed as an enum</returns>
        private static ushort DeserializeMessageSubType(IEnumerable<byte> data)
        {
            var subTypeArray = data.Skip(MessageConstants.MessageTypeLength).Take(MessageConstants.MessageSubTypeLength).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(subTypeArray);
            var subType = BitConverter.ToUInt16(subTypeArray, 0);
            return subType;
        }

        /// <summary>
        /// Retrieves the message length
        /// </summary>
        /// <param name="data">Full message data</param>
        /// <returns>Message length</returns>
        private static uint DeserializeMessageLength(IEnumerable<byte> data)
        {
            var lengthArray = data.Skip(MessageConstants.MessageTypeLength + MessageConstants.MessageSubTypeLength)
                .Take(MessageConstants.MessageLengthLength).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthArray);
            var length = BitConverter.ToUInt32(lengthArray, 0);
            return length;
        }

        /// <summary>
        /// Retrieves the message length
        /// </summary>
        /// <param name="data">Full message data</param>
        /// <returns>Message is compressed or not</returns>
        private static bool DeserializeMessageCompressed(IEnumerable<byte> data)
        {
            var compressedArray = data.Skip(MessageConstants.MessageTypeLength + MessageConstants.MessageSubTypeLength + MessageConstants.MessageLengthLength)
                .Take(MessageConstants.MessageCompressionValueLength).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(compressedArray);
            var isCompressed = BitConverter.ToBoolean(compressedArray, 0);
            return isCompressed;
        }


        /// <summary>
        /// Retrieves a message from the dictionary based on the type
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <param name="messageSubType">Message subtype</param>
        /// <param name="content">Message content</param>
        /// <param name="dataCompressed">Is data compresssed?</param>
        /// <returns></returns>
        private IMessageBase GetMessageByType(ushort messageType, ushort messageSubType, byte[] content, bool dataCompressed)
        {
            if (Enum.IsDefined(HandledMessageTypes, (int)messageType) && MessageDictionary.ContainsKey(messageType))
            {
                var msg = MessageStore.GetMessage(MessageDictionary[messageType]);
                msg.SetData(msg.Deserialize(messageSubType, content, dataCompressed));

                return msg;
            }
            throw new Exception("Cannot deserialize this type of message!");
        }
    }
}