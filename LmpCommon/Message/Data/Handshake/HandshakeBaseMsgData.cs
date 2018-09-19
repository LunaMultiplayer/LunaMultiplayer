using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Handshake
{
    public abstract class HandshakeBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal HandshakeBaseMsgData() { }
        public override ushort SubType => (ushort)(int)HandshakeMessageType;

        public virtual HandshakeMessageType HandshakeMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}
