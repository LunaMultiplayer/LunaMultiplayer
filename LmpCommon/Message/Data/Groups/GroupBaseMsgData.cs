using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Groups
{
    public abstract class GroupBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal GroupBaseMsgData() { }
        public override bool CompressCondition => false;
        public override ushort SubType => (ushort)(int)GroupMessageType;

        public virtual GroupMessageType GroupMessageType => throw new NotImplementedException();

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