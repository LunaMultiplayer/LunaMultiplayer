using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Server.Base;
using System;

namespace LmpCommon.Message
{
    /// <summary>
    /// Class for deserialization of the server messages
    /// </summary>
    public class ServerMessageFactory : FactoryBase
    {
        protected internal override Type HandledMessageTypes { get; } = typeof(ServerMessageType);
        protected internal override Type BaseMsgType => typeof(SrvMsgBase<>);
    }
}