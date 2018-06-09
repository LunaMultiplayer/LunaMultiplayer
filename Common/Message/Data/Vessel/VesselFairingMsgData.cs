using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselFairingMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselFairingMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Fairing;
        
        public uint PartFlightId;
        
        public override string ClassName { get; } = nameof(VesselFairingMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint);
        }
    }
}
