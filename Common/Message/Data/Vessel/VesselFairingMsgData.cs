using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselFairingMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselFairingMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Fairing;
        
        public Guid VesselId;
        public uint PartFlightId;
        
        public override string ClassName { get; } = nameof(VesselFairingMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(PartFlightId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            PartFlightId = lidgrenMsg.ReadUInt32();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + GuidUtil.GetByteSize() + sizeof(uint);
        }
    }
}