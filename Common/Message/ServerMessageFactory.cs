using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Server.Base;
using System;

namespace LunaCommon.Message
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