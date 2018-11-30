using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselDecoupleMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselDecoupleMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Decouple;

        public uint PartFlightId;
        public float BreakForce;
        public Guid NewVesselId;

        public override string ClassName { get; } = nameof(VesselDecoupleMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(BreakForce);
            GuidUtil.Serialize(NewVesselId, lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            BreakForce = lidgrenMsg.ReadFloat();
            NewVesselId = GuidUtil.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint) + sizeof(float) + GuidUtil.ByteSize;
        }
    }
}
