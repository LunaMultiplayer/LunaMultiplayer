using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.MasterServer.Base;
using System;

namespace LunaCommon.Message
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