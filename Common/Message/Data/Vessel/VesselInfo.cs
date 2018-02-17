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

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            GuidUtil.Serialize(VesselId, lidgrenMsg);

            //TODO: This creates an issue!! The problem is that the server reads this message (so it deserializes it) 
            //and then it directly relays it so it's not properly compressed. Then on the client it tries to decompress it and the message is corrupted.
            //As a result the message becomes corrupt

            //Array.Resize(ref Data, NumBytes);
            //Data = CompressionHelper.compress(Data, 3);
            //NumBytes = Data.Length;

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            VesselId = GuidUtil.Deserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            //Array.Resize(ref Data, NumBytes);
            //Data = CompressionHelper.decompress(Data);
            //NumBytes = Data.Length;
        }

        public int GetByteCount()
        {
            return GuidUtil.GetByteSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
