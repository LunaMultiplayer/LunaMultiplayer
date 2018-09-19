using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Screenshot
{
    public class ScreenshotInfo
    {
        public string FolderName;
        public long DateTaken;

        public int NumBytes;
        public byte[] Data = new byte[0];
        public ushort Width;
        public ushort Height;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(DateTaken);
            lidgrenMsg.Write(Width);
            lidgrenMsg.Write(Height);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FolderName = lidgrenMsg.ReadString();
            DateTaken = lidgrenMsg.ReadInt64();

            Width = lidgrenMsg.ReadUInt16();
            Height = lidgrenMsg.ReadUInt16();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return FolderName.GetByteCount() + sizeof(long) + sizeof(ushort) * 2 + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
