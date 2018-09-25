using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselResourceInfo
    {
        public uint PartFlightId;
        public string ResourceName;
        public double Amount;
        public bool FlowState;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(ResourceName);
            lidgrenMsg.Write(Amount);
            lidgrenMsg.Write(FlowState);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PartFlightId = lidgrenMsg.ReadUInt32();
            ResourceName = lidgrenMsg.ReadString();
            Amount = lidgrenMsg.ReadDouble();
            FlowState = lidgrenMsg.ReadBoolean();
        }

        public int GetByteCount()
        {
            return sizeof(uint) + ResourceName.GetByteCount() + sizeof(double) + sizeof(bool);
        }
    }
}
