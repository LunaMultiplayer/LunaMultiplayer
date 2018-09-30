using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.ShareProgress
{
    public abstract class ShareProgressBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ShareProgressBaseMsgData() { }
        public override ushort SubType => (ushort)(int)ShareProgressMessageType;
        public virtual ShareProgressMessageType ShareProgressMessageType => throw new NotImplementedException();

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
