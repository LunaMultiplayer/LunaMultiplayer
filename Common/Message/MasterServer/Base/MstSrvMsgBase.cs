using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Interface;

namespace LunaCommon.Message.MasterServer.Base
{
    /// <summary>
    ///     Basic master server message
    /// </summary>
    public abstract class MstSrvMsgBase<T> : MessageBase<T>, IMasterServerMessageBase
        where T : IMessageData, new()
    {
        protected override ushort MessageTypeId => (ushort)(int)MessageType;

        /// <summary>
        ///     Message type as a master server type
        /// </summary>
        public abstract MasterServerMessageType MessageType { get; }
    }
}