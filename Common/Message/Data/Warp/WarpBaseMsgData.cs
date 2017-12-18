using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Warp
{
    public abstract class WarpBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal WarpBaseMsgData() { }
        public override ushort SubType => (ushort)(int)WarpMessageType;
        public virtual WarpMessageType WarpMessageType => throw new NotImplementedException();

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