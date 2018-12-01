using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselCoupleMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselCoupleMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Couple;

        public uint PartFlightId;
        public Guid CoupledVesselId;
        public uint CoupledPartFlightId;
        public int SubspaceId;
        public int Trigger;

        public override string ClassName { get; } = nameof(VesselCoupleMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            GuidUtil.Serialize(CoupledVesselId, lidgrenMsg);
            lidgrenMsg.Write(CoupledPartFlightId);
            lidgrenMsg.Write(SubspaceId);
            lidgrenMsg.Write(Trigger);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            CoupledVesselId = GuidUtil.Deserialize(lidgrenMsg);
            CoupledPartFlightId = lidgrenMsg.ReadUInt32();
            SubspaceId = lidgrenMsg.ReadInt32();
            Trigger = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint) * 2 + GuidUtil.ByteSize + sizeof(int) * 2;
        }
    }
}
