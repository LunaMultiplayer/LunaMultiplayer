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