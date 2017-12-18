using Lidgren.Network;
using LunaCommon.Message.Interface;

namespace LunaCommon.Message.Base
{
    public abstract class MessageData : IMessageData
    {
        /// <summary>
        /// Make constructor internal so they have to use the factory.
        /// This is made this way as the factory use a cache system to avoid the generation of garbage
        /// </summary>
        internal MessageData() { }

        /// <inheritdoc />
        public virtual string Version { get; set; } = LmpVersioning.CurrentVersion;

        /// <inheritdoc />
        public long ReceiveTime { get; set; }

        /// <inheritdoc />
        public long SentTime { get; set; }

        /// <inheritdoc />
        public virtual ushort SubType => 0;

        /// <inheritdoc />
        public abstract string ClassName { get; }

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(SentTime);
            lidgrenMsg.Write(Version);
            InternalSerialize(lidgrenMsg, dataCompressed);
        }

        internal abstract void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed);

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            SentTime = lidgrenMsg.ReadInt64();
            Version = lidgrenMsg.ReadString();
            InternalDeserialize(lidgrenMsg, dataCompressed);
        }

        internal abstract void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed);

        public abstract void Recycle();

        public int GetMessageSize(bool dataCompressed)
        {
            return sizeof(long) + Version.GetByteCount() + InternalGetMessageSize(dataCompressed);
        }

        internal abstract int InternalGetMessageSize(bool dataCompressed);
    }
}