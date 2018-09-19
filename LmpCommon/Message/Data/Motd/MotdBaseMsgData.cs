using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Motd
{
    public abstract class MotdBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal MotdBaseMsgData() { }
        public override ushort SubType => (ushort)(int)MotdMessageType;
        public virtual MotdMessageType MotdMessageType => throw new NotImplementedException();

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
