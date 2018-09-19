using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Interface;

namespace LmpCommon.Message.Server.Base
{
    /// <summary>
    ///     Basic server message
    /// </summary>
    public abstract class SrvMsgBase<T> : MessageBase<T>, IServerMessageBase
        where T : class, IMessageData
    {
        /// <inheritdoc />
        internal SrvMsgBase() { }

        protected override ushort MessageTypeId => (ushort)(int)MessageType;

        /// <summary>
        ///     Message type as a server type
        /// </summary>
        public abstract ServerMessageType MessageType { get; }
    }
}