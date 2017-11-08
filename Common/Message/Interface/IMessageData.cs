namespace LunaCommon.Message.Interface
{
    public interface IMessageData
    {
        /// <summary>
        /// Retrieves the version of the multiplayer
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Receive time timestamp.
        /// </summary>
        long ReceiveTime { get; set; }

        /// <summary>
        /// Subtype message identifier (Chat messages for example)
        /// </summary>
        ushort SubType { get; }

        /// <summary>
        /// Sent time timestamp.
        /// </summary>
        long SentTime { get; set; }

        /// <summary>
        /// This field is set to true after the message has been used so it can be recycled
        /// </summary>
        bool ReadyToRecycle { get; set; }
    }
}