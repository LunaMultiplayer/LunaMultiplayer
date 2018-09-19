using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Client.Base;
using System;

namespace LmpCommon.Message
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