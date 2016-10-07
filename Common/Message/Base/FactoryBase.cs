using System;
using System.Collections.Generic;
using System.Linq;
using LunaCommon.Message.Interface;

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
        ///     Call this method to deserialize a message
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
            catch (Exception e)
            {
                //Bad message, we couldn't deserialize
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
        protected Dictionary<uint, IMessageBase> MessageDictionary { get; } = new Dictionary<uint, IMessageBase>();

        /// <summary>
        ///     Call this method to serialize a message to an array of bytes
        /// </summary>
        /// <param name="message">Message to serialize</param>
        /// <returns>The message as an array of bytes ready to be sent</returns>
        public byte[] Serialize<T>(T message) where T : IMessageBase, new()
        {
            return message.Serialize(_compress);
        }

        /// <summary>
        ///     Method to retrieve a new message
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <returns>New empty message</returns>
        public T CreateNew<T>() where T : IMessageBase, new()
        {
            return new T();
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T CreateNew<T>(IMessageData data) where T : IMessageBase, new()
        {
            var msg = new T();
            msg.SetData(data);
            return msg;
        }

        /// <summary>
        ///     Retrieves the message type
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
        ///     Retrieves the message sub-type
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
        ///     Retrieves the message length
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
        ///     Retrieves the message length
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
                var msg = MessageDictionary[messageType].Clone();
                msg.SetData(msg.Deserialize(messageSubType, content, dataCompressed));

                return msg;
            }
            throw new Exception("Cannot deserialize this type of message!");
        }
    }
}