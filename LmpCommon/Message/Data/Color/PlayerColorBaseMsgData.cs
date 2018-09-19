using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Color
{
    public abstract class PlayerColorBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerColorBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerColorMessageType;
        public virtual PlayerColorMessageType PlayerColorMessageType => throw new NotImplementedException();

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