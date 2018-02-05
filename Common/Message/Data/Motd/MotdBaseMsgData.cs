using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Motd
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
