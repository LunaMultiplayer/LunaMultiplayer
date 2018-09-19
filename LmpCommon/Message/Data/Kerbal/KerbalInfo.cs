using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Kerbal
{
    public class KerbalInfo
    {
        public string KerbalName;
        public int NumBytes;
        public byte[] KerbalData = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(KerbalName);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(KerbalData, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            KerbalName = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (KerbalData.Length < NumBytes)
                KerbalData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(KerbalData, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return KerbalName.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
