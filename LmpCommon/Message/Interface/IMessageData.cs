using Lidgren.Network;

namespace LmpCommon.Message.Interface
{
    public interface IMessageData
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// Retrieves the major version of the multiplayer
        /// </summary>
        ushort MajorVersion { get; }

        /// <summary>
        /// Retrieves the minor version of the multiplayer
        /// </summary>
        ushort MinorVersion { get; }

        /// <summary>
        /// Retrieves the build version of the multiplayer
        /// </summary>
        ushort BuildVersion { get; }

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
        /// Serializes this message to the NetOutgoingMessage
        /// </summary>
        void Serialize(NetOutgoingMessage lidgrenMsg);

        /// <summary>
        /// Deserializes a message from the NetIncomingMessage
        /// </summary>
        void Deserialize(NetIncomingMessage lidgrenMsg);

        /// <summary>
        /// Size of this data in bytes
        /// </summary>
        int GetMessageSize();
    }
}