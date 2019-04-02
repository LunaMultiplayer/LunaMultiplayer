using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Flag
{
    public abstract class FlagBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FlagBaseMsgData() { }
        public override ushort SubType => (ushort)(int)FlagMessageType;
        public virtual FlagMessageType FlagMessageType => throw new NotImplementedException();

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