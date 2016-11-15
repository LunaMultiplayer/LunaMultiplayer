using System;
using System.Collections.Generic;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Serialization;
using Lidgren.Network;

namespace LunaCommon.Message.Base
{
    /// <summary>
    ///     Basic message class
    /// </summary>
    /// <typeparam name="T">POCO message data class with the message properties</typeparam>
    public abstract class MessageBase<T> : IMessageBase
        where T : IMessageData, new()
    {
        private IMessageData _data;

        /// <summary>
        /// Override this dictionary if your message has several subtypes (Check chat for example). The key in this case is the SUBTYPE id
        /// </summary>
        protected virtual Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [0] = new T()
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

        /// <summary>
        ///     Sets the data of this message
        /// </summary>
        /// <param name="data"></param>
        public void SetData(IMessageData data)
        {
            Data = data;
        }

        public IMessageBase Clone()
        {
            return MemberwiseClone() as IMessageBase;
        }

        /// <summary>
        ///     POCO class with the data that it handles
        ///     The setter is private to keep the factory pattern
        ///     Use "SetData" method to set it's data value
        /// </summary>
        public IMessageData Data
        {
            get { return _data; }
            private set
            {
                if (!(value is T) && (typeof(T) != value.GetType()))
                    throw new InvalidOperationException("Cannot cast this mesage data to this type of message");
                _data = value;
            }
        }

        /// <summary>
        ///     Public accessor to retrieve the Channel correctly
        /// </summary>
        /// <returns></returns>
        public int Channel
        {
            get
            {
                if ((NetDeliveryMethod == NetDeliveryMethod.Unreliable) ||
                    (NetDeliveryMethod == NetDeliveryMethod.ReliableUnordered))
                    return 0;
                if (DefaultChannel > 32)
                    throw new Exception("Cannot set a channel above 32!");
                return DefaultChannel;
            }
        }

        /// <summary>
        ///     Specify how the message should be delivered based on lidgren library.
        ///     This is important to avoid lag!
        ///     Unreliable: No guarantees. (Use for unimportant messages like heartbeats)
        ///     UnreliableSequenced: Late messages will be dropped if newer ones were already received.
        ///     ReliableUnordered: All packages will arrive, but not necessarily in the same order.
        ///     ReliableSequenced: All packages will arrive, but late ones will be dropped.
        ///     This means that we will always receive the latest message eventually, but may miss older ones.
        ///     ReliableOrdered: All packages will arrive, and they will do so in the same order.
        ///     Unlike all the other methods, here the library will hold back messages until all previous ones are received,
        ///     before handing them to us.
        /// </summary>
        public abstract NetDeliveryMethod NetDeliveryMethod { get; }

        /// <summary>
        ///     This method creates a POCO object based on the array without the header
        /// </summary>
        /// <param name="subType">The subtype of the message data to deserialize</param>
        /// <param name="data">The compressed data to read from. Without the header</param>
        /// <param name="decompress">Decompress the data or not</param>
        /// <returns>The POCO data structure with it's properties filled</returns>
        public virtual IMessageData Deserialize(ushort subType, byte[] data, bool decompress)
        {
            if (!SubTypeDictionary.ContainsKey(subType))
            {
                throw new Exception("Subtype not defined in dictionary!");
            }

            var dataClass = SubTypeDictionary[subType].Clone();
            if (decompress)
            {
                var decompressed = CompressionHelper.DecompressBytes(data);
                return DataDeserializer.Deserialize(this, dataClass, decompressed);
            }

            return DataDeserializer.Deserialize(this, dataClass, data);
        }

        /// <summary>
        ///     True if the data version property mismatches
        /// </summary>
        public bool VersionMismatch { get; set; }

        /// <summary>
        ///     This method retrieves the message as a byte array with it's 9 byte header at the beginning and it's data compressed
        ///     if the size is reduced
        /// </summary>
        /// <param name="compress">Compress the message or not</param>
        /// <returns>Mesage as a byte array with it's header</returns>
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

                return fullData;
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing message! MsgDataType: {Data.GetType()} Exception: {e}");
            }
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