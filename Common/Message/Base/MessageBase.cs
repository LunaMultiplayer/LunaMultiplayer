using Lidgren.Network;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using DataDeserializer = LunaCommon.Message.Serialization.DataDeserializer;
using DataSerializer = LunaCommon.Message.Serialization.DataSerializer;

namespace LunaCommon.Message.Base
{
    /// <summary>
    ///     Basic message class
    /// </summary>
    /// <typeparam name="T">POCO message data class with the message properties</typeparam>
    public abstract class MessageBase<T> : IMessageBase
        where T : class, IMessageData
    {
        private IMessageData _data;

        /// <summary>
        /// Make constructor internal so they have to use the factory.
        /// This is made this way as the factory use a cache system to avoid the generation of garbage
        /// </summary>
        internal MessageBase() { }

        /// <summary>
        /// Override this dictionary if your message has several subtypes (Check chat for example). The key in this case is the SUBTYPE id
        /// </summary>
        protected virtual Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [0] = typeof(T)
        };

        /// <summary>
        ///     Message type. Max message types are 65535
        /// </summary>
        protected abstract ushort MessageTypeId { get; }

        /// <summary>
        ///     This parameter can be used to specify a channel for the lidgren delivery methods that preserve order
        ///     (whether by holding back, or dropping messages received out of order).
        ///     Any messages sent with the same channel will respect each other’s order, while messages sent with different
        ///     channels will
        ///     not interfere with each other.
        ///     This can be useful to not have unrelated kinds of messages interfere with each other.
        ///     For example, player health point and position updates should probably not be send using the same channel,
        ///     since they are in essence independent of each other, and we care about receiving up-to-date information of both.
        ///     If the Delivery method is unreliable, the channel will be defaulted to 0
        /// </summary>
        protected abstract int DefaultChannel { get; }

        /// <inheritdoc />
        public void SetData(IMessageData data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public IMessageData Data
        {
            get => _data;
            private set
            {
                if (!(value is T) && typeof(T) != value.GetType())
                    throw new InvalidOperationException("Cannot cast this mesage data to this type of message");
                _data = value;
            }
        }

        /// <inheritdoc />
        public int Channel
        {
            get
            {
                if (NetDeliveryMethod == NetDeliveryMethod.Unreliable ||
                    NetDeliveryMethod == NetDeliveryMethod.ReliableUnordered)
                    return 0;
                if (DefaultChannel > 32)
                    throw new Exception("Cannot set a channel above 32!");
                return DefaultChannel;
            }
        }

        /// <inheritdoc />
        public abstract NetDeliveryMethod NetDeliveryMethod { get; }

        /// <inheritdoc />
        public virtual IMessageData Deserialize(ushort subType, byte[] data, bool decompress)
        {
            if (!SubTypeDictionary.ContainsKey(subType))
            {
                throw new Exception("Subtype not defined in dictionary!");
            }

            var msgDataType = SubTypeDictionary[subType];
            var msgData = MessageStore.GetMessageData(msgDataType);

            if (decompress)
            {
                var decompressed = CompressionHelper.DecompressBytes(data);
                return DataDeserializer.Deserialize(this, msgData, decompressed);
            }

            return DataDeserializer.Deserialize(this, msgData, data);
        }

        /// <inheritdoc />
        public bool VersionMismatch { get; set; }

        /// <inheritdoc />
        public byte[] Serialize(bool compress)
        {
            try
            {
                var data = DataSerializer.Serialize(Data) ?? new byte[0];

                if (compress)
                {
                    var dataCompressed = CompressionHelper.CompressBytes(data);

                    compress = dataCompressed != null && dataCompressed.Length < data.Length;
                    if (compress)
                    {
                        data = dataCompressed;
                    }
                }

                var header = SerializeHeaderData(Convert.ToUInt32(data.Length), compress);

                var fullData = new byte[header.Length + data.Length];
                header.CopyTo(fullData, 0);
                data.CopyTo(fullData, header.Length);
                
                MessageStore.RecycleMessage(this);

                return fullData;
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing message! MsgDataType: {Data.GetType()} Exception: {e}");
            }
        }

        public void Recycle()
        {
            MessageStore.RecycleMessage(this);
        }

        /// <summary>
        ///     Serializes the header information
        /// </summary>
        /// <param name="dataLength">Length of the message data without the header</param>
        /// <param name="dataCompressed">Is data compressed or not</param>
        /// <returns>Byte with the header serialized</returns>
        private byte[] SerializeHeaderData(uint dataLength, bool dataCompressed)
        {
            var headerData = new byte[MessageConstants.HeaderLength];

            var typeBytes = BitConverter.GetBytes(MessageTypeId);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(typeBytes);
            typeBytes.CopyTo(headerData, MessageConstants.MessageTypeStartIndex);

            var subTypeBytes = BitConverter.GetBytes(Data.SubType);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(subTypeBytes);
            subTypeBytes.CopyTo(headerData, MessageConstants.MessageSubTypeStartIndex);

            var lengthBytes = BitConverter.GetBytes(dataLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            lengthBytes.CopyTo(headerData, MessageConstants.MessageLengthStartIndex);

            var compressedByte = BitConverter.GetBytes(dataCompressed);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(compressedByte);
            compressedByte.CopyTo(headerData, MessageConstants.MessageCompressionValueIndex);

            return headerData;
        }
    }
}