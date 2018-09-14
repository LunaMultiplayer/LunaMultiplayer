using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselResourceInfo
    {
        public uint PartFlightId;
        public uint PartPersistentId;
        public string ResourceName;
        public double Amount;
        public bool FlowState;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(PartPersistentId);
            lidgrenMsg.Write(ResourceName);
            lidgrenMsg.Write(Amount);
            lidgrenMsg.Write(FlowState);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PartFlightId = lidgrenMsg.ReadUInt32();
            PartPersistentId = lidgrenMsg.ReadUInt32();
            ResourceName = lidgrenMsg.ReadString();
            Amount = lidgrenMsg.ReadDouble();
            FlowState = lidgrenMsg.ReadBoolean();
        }

        public int GetByteCount()
        {
            return sizeof(uint) * 2 + ResourceName.GetByteCount() + sizeof(double) + sizeof(bool);
        }
    }
}
