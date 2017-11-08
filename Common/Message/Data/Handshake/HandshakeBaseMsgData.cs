using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal HandshakeBaseMsgData() { }
        public override ushort SubType => (ushort)(int)HandshakeMessageType;

        public virtual HandshakeMessageType HandshakeMessageType => throw new NotImplementedException();
    }
}
