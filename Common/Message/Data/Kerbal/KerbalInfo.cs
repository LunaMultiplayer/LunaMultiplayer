using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalInfo
    {
        public string KerbalName;
        public int NumBytes;
        public byte[] KerbalData = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(KerbalName);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(KerbalData, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            KerbalName = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            KerbalData = ArrayPool<byte>.Claim(NumBytes);
            lidgrenMsg.ReadBytes(KerbalData, 0, NumBytes);
        }

        public void Recycle()
        {
            ArrayPool<byte>.Release(ref KerbalData);
        }

        public int GetByteCount(bool dataCompressed)
        {
            return KerbalName.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
