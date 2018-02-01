using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Admin
{
    public abstract class AdminBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal AdminBaseMsgData() { }
        public override ushort SubType => (ushort)(int)AdminMessageType;
        public virtual AdminMessageType AdminMessageType => throw new NotImplementedException();

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