using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Groups
{
    public abstract class GroupBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal GroupBaseMsgData() { }
        public override ushort SubType => (ushort)(int)GroupMessageType;

        public virtual GroupMessageType GroupMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
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