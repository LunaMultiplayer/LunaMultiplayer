using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Interface;

namespace LmpCommon.Message.MasterServer.Base
{
    /// <summary>
    ///     Basic master server message
    /// </summary>
    public abstract class MstSrvMsgBase<T> : MessageBase<T>, IMasterServerMessageBase
        where T : class, IMessageData
    {
        protected override ushort MessageTypeId => (ushort)(int)MessageType;

        /// <summary>
        ///     Message type as a master server type
        /// </summary>
        public abstract MasterServerMessageType MessageType { get; }
    }
}