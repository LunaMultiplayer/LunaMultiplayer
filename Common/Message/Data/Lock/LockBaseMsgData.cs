using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Lock
{
    public abstract class LockBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal LockBaseMsgData() { }
        public override ushort SubType => (ushort)(int)LockMessageType;
        public virtual LockMessageType LockMessageType => throw new NotImplementedException();

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