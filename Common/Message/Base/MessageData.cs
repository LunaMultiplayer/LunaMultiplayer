using LunaCommon.Message.Interface;

namespace LunaCommon.Message.Base
{
    public class MessageData : IMessageData
    {
        public IMessageData Clone()
        {
            return MemberwiseClone() as IMessageData;
        }

        /// <summary>
        ///     Returns the current version number
        /// </summary>
        public virtual string Version => VersionInfo.VersionNumber;

        /// <summary>
        /// Receive time timestamp, useful for interpolation.
        /// </summary>
        public long ReceiveTime { get; set; }

        /// <summary>
        ///     Returns the message subtype, override in cases that you need, chat for example
        /// </summary>
        public virtual ushort SubType => 0;
    }
}