using Lidgren.Network;
using LunaCommon.Message.Base;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselInfo
    {
        public Guid VesselId;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            GuidUtil.Serialize(VesselId, lidgrenMsg);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            VesselId = GuidUtil.Deserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }
        
        public int GetByteCount(bool dataCompressed)
        {
            return GuidUtil.GetByteSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
