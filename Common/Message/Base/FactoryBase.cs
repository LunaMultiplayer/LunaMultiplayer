using Lidgren.Network;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.IO;

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
            var data = ArrayPool<byte>.Claim(lidgrenMsg.LengthBytes);
            lidgrenMsg.ReadBytes(data, 0, lidgrenMsg.LengthBytes);

            using (var stream = StreamManager.MemoryStreamManager.GetStream("", data, 0, lidgrenMsg.LengthBytes))
            {
                if (lidgrenMsg.LengthBytes >= MessageConstants.HeaderLength)
                {
                    var messageType = DeserializeMessageType(stream);
                    var subtype = DeserializeMessageSubType(stream);
                    var length = DeserializeMessageLength(stream);
                    var dataCompressed = DeserializeMessageCompressed(stream);

                    if (stream.Length - MessageConstants.HeaderLength == length)
                    {
                        var msg = GetMessageByType(messageType, subtype, stream, dataCompressed);
                        msg.Data.ReceiveTime = receiveTime;

                        ArrayPool<byte>.Release(ref data);
                        return msg;
                    }
                    ArrayPool<byte>.Release(ref data);
                    throw new Exception("Message data size mismatch");
                }
                ArrayPool<byte>.Release(ref data);
                throw new Exception("Message length below header size");
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
        /// Retrieves the message type
        /// </summary>
        /// <param name="stream">Full message stream</param>
        /// <returns>Message type to be parsed as an enum</returns>
        private static ushort DeserializeMessageType(Stream stream)
        {
            var typeArray = ArrayPool<byte>.Claim(MessageConstants.MessageTypeLength);

            stream.Read(typeArray, 0, MessageConstants.MessageTypeLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(typeArray);
            var type = BitConverter.ToUInt16(typeArray, 0);

            ArrayPool<byte>.Release(ref typeArray);
            return type;
        }

        /// <summary>
        /// Retrieves the message sub-type
        /// </summary>
        /// <param name="stream">Full message stream</param>
        /// <returns>Message sub-type to be parsed as an enum</returns>
        private static ushort DeserializeMessageSubType(Stream stream)
        {
            var subTypeArray = ArrayPool<byte>.Claim(MessageConstants.MessageSubTypeLength);

            stream.Read(subTypeArray, 0, MessageConstants.MessageTypeLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(subTypeArray);
            var subType = BitConverter.ToUInt16(subTypeArray, 0);

            ArrayPool<byte>.Release(ref subTypeArray);
            return subType;
        }

        /// <summary>
        /// Retrieves the message length
        /// </summary>
        /// <param name="stream">Full message stream</param>
        /// <returns>Message length</returns>
        private static uint DeserializeMessageLength(Stream stream)
        {
            var lengthArray = ArrayPool<byte>.Claim(MessageConstants.MessageLengthLength);

            stream.Read(lengthArray, 0, MessageConstants.MessageLengthLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthArray);
            var length = BitConverter.ToUInt32(lengthArray, 0);

            ArrayPool<byte>.Release(ref lengthArray);
            return length;
        }

        /// <summary>
        /// Retrieves the message length
        /// </summary>
        /// <param name="stream">Full message stream</param>
        /// <returns>Message is compressed or not</returns>
        private static bool DeserializeMessageCompressed(Stream stream)
        {
            var compressedArray = ArrayPool<byte>.Claim(MessageConstants.MessageCompressionValueLength);

            stream.Read(compressedArray, 0, MessageConstants.MessageCompressionValueLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(compressedArray);
            var isCompressed = BitConverter.ToBoolean(compressedArray, 0);

            ArrayPool<byte>.Release(ref compressedArray);
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
        private IMessageBase GetMessageByType(ushort messageType, ushort messageSubType, MemoryStream content, bool dataCompressed)
        {
            if (Enum.IsDefined(HandledMessageTypes, (int)messageType) && MessageDictionary.ContainsKey(messageType))
            {
                var msg = MessageStore.GetMessage(MessageDictionary[messageType]);
                var data = msg.Deserialize(messageSubType, content, dataCompressed);
                msg.SetData(data);

                return msg;
            }
            throw new Exception("Cannot deserialize this type of message!");
        }
    }
}