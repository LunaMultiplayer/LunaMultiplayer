using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using System;

namespace LunaCommon.Message
{
    /// <summary>
    /// Class for deserialization of the client messages
    /// </summary>
    public class ClientMessageFactory : FactoryBase
    {
        protected internal override Type BaseMsgType => typeof(CliMsgBase<>);
        protected internal override Type HandledMessageTypes { get; } = typeof(ClientMessageType);
    }
}