using Lidgren.Network;
using LunaCommon.Compression;
using LunaCommon.Message.Base;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselInfo
    {
        public Guid VesselId;
        public int VesselSituation;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(VesselSituation);

            Array.Resize(ref Data, NumBytes);
            Data = CompressionHelper.compress(Data, 3);
            NumBytes = Data.Length;

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            VesselSituation = lidgrenMsg.ReadInt32();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            Array.Resize(ref Data, NumBytes);
            Data = CompressionHelper.decompress(Data);
            NumBytes = Data.Length;
        }

        public int GetByteCount()
        {
            return GuidUtil.GetByteSize() + sizeof(int) * 2 + sizeof(byte) * NumBytes;
        }
    }
}
