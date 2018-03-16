using Lidgren.Network;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotInfo
    {
        public long DateTaken;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(DateTaken);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            DateTaken = lidgrenMsg.ReadInt64();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return sizeof(long) + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
