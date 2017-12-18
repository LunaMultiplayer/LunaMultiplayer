using Lidgren.Network;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;

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


        /// <inheritdoc />
        public virtual bool AvoidCompression { get; } = false;

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
                if (value != null && !(value is T) && typeof(T) != value.GetType())
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
        public virtual IMessageData GetMessageData(ushort subType)
        {
            if (!SubTypeDictionary.ContainsKey(subType))
            {
                throw new Exception("Subtype not defined in dictionary!");
            }

            var msgDataType = SubTypeDictionary[subType];
            return MessageStore.GetMessageData(msgDataType);
        }

        /// <inheritdoc />
        public bool VersionMismatch { get; set; }

        /// <inheritdoc />
        public void Serialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            try
            {
                lidgrenMsg.Write(MessageTypeId);
                lidgrenMsg.Write(Data.SubType);
                lidgrenMsg.Write(compressData);

                Data.Serialize(lidgrenMsg, compressData);
            }
            catch (Exception e)
            {
                throw new Exception($"Error serializing message! MsgDataType: {Data.GetType()} Exception: {e}");
            }
        }

        /// <inheritdoc />
        public void Recycle()
        {
            Data.Recycle();
            MessageStore.RecycleMessage(this);
        }

        /// <inheritdoc />
        public int GetMessageSize(bool dataCompressed)
        {
            return sizeof(ushort) + sizeof(ushort) + sizeof(bool) + Data.GetMessageSize(dataCompressed);
        }
    }
}