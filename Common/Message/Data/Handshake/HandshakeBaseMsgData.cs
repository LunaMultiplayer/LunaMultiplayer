using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)HandshakeMessageType;

        public virtual HandshakeMessageType HandshakeMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
