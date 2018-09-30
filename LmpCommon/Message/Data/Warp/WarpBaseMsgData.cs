using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Warp
{
    public abstract class WarpBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal WarpBaseMsgData() { }
        public override bool CompressCondition => false;
        public override ushort SubType => (ushort)(int)WarpMessageType;
        public virtual WarpMessageType WarpMessageType => throw new NotImplementedException();

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