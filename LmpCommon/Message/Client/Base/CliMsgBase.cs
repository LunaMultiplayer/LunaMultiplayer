using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Interface;

namespace LmpCommon.Message.Client.Base
{
    /// <summary>
    ///     Basic client message
    /// </summary>
    public abstract class CliMsgBase<T> : MessageBase<T>, IClientMessageBase
        where T : class, IMessageData
    {
        /// <inheritdoc />
        internal CliMsgBase() { }

        protected override ushort MessageTypeId => (ushort)(int) MessageType;

        /// <summary>
        ///     Message type as a client type
        /// </summary>
        public abstract ClientMessageType MessageType { get; }

        /// <summary>
        ///     Use this property and set it to true when your mod handles this message
        /// </summary>
        public bool Handled { get; set; }
    }
}