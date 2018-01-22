using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Handshake
{
    public abstract class HandshakeBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal HandshakeBaseMsgData() { }
        public override ushort SubType => (ushort)(int)HandshakeMessageType;

        public virtual HandshakeMessageType HandshakeMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return 0;
        }
    }
}
