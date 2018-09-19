using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.MasterServer.Base;
using System;

namespace LmpCommon.Message
{
    /// <summary>
    /// Class for deserialization of the master server messages
    /// </summary>
    public class MasterServerMessageFactory : FactoryBase
    {
        protected internal override Type HandledMessageTypes { get; } = typeof(ServerMessageType);
        protected internal override Type BaseMsgType => typeof(MstSrvMsgBase<>);
    }
}