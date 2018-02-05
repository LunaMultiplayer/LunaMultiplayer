using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.MasterServer
{
    public abstract class MsBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal MsBaseMsgData() { }
        public override ushort SubType => (ushort)(int)MasterServerMessageSubType;
        public virtual MasterServerMessageSubType MasterServerMessageSubType => throw new NotImplementedException();

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
