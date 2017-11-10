using LunaCommon.Message.Interface;

namespace LunaCommon.Message.Base
{
    public class MessageData : IMessageData
    {
        /// <summary>
        /// Make constructor internal so they have to use the factory.
        /// This is made this way as the factory use a cache system to avoid the generation of garbage
        /// </summary>
        internal MessageData() { }
        
        /// <inheritdoc />
        public virtual string Version => Common.CurrentVersion;

        /// <inheritdoc />
        public long ReceiveTime { get; set; }

        /// <inheritdoc />
        public long SentTime { get; set; }

        /// <inheritdoc />
        public virtual ushort SubType => 0;

        /// <inheritdoc />
        public bool ReadyToRecycle { get; set; }
    }
}