using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public abstract class VesselBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal VesselBaseMsgData() { }
        public override ushort SubType => (ushort)(int)VesselMessageType;
        public virtual VesselMessageType VesselMessageType => throw new NotImplementedException();

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
