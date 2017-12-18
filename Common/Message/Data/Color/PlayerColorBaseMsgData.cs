using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Color
{
    public abstract class PlayerColorBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerColorBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerColorMessageType;
        public virtual PlayerColorMessageType PlayerColorMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        public override void Recycle()
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return 0;
        }
    }
}