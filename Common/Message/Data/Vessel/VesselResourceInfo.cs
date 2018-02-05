using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselResourceInfo
    {
        public uint PartFlightId;
        public string ResourceName;
        public double Amount;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(ResourceName);
            lidgrenMsg.Write(Amount);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PartFlightId = lidgrenMsg.ReadUInt32();
            ResourceName = lidgrenMsg.ReadString();
            Amount = lidgrenMsg.ReadDouble();
        }

        public int GetByteCount()
        {
            return sizeof(uint) + ResourceName.GetByteCount() + sizeof(double);
        }
    }
}
