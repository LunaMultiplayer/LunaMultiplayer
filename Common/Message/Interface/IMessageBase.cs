using Lidgren.Network;

namespace LunaCommon.Message.Interface
{
    public interface IMessageBase
    {
        /// <summary>
        /// This method is made to clone the message and use a memberwise copy
        /// </summary>
        IMessageBase Clone();

        /// <summary>
        ///     POCO class with the data that it handles
        /// </summary>
        IMessageData Data { get; }

        /// <summary>
        ///     True if the data version property mismatches
        /// </summary>
        bool VersionMismatch { get; set; }

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
        NetDeliveryMethod NetDeliveryMethod { get; }

        /// <summary>
        ///     Public accessor to retrieve the Channel correctly
        /// </summary>
        /// <returns></returns>
        int Channel { get; }

        void SetData(IMessageData data);

        /// <summary>
        ///     This method creates a POCO object based on the array without the header
        /// </summary>
        /// <param name="messageSubType">Message subtype (Chat-console is a subtype of chatbase for example)</param>
        /// <param name="data">The compressed data to read from. Without the header</param>
        /// <param name="decompress">Decompress the data or not</param>
        /// <returns>The POCO data structure with it's properties filled</returns>
        IMessageData Deserialize(ushort messageSubType, byte[] data, bool decompress);

        /// <summary>
        ///     This method retrieves the message as a byte array with it's 8 byte header at the beginning
        /// </summary>
        /// <param name="compress">Compress the message or not</param>
        /// <returns>Mesage as a byte array with it's header</returns>
        byte[] Serialize(bool compress);
    }
}