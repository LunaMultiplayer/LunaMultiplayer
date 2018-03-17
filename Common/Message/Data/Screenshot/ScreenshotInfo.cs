using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotInfo
    {
        public string Folder;
        public long DateTaken;

        public int NumBytes;
        public byte[] Data = new byte[0];
        public ushort Width;
        public ushort Height;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Folder);
            lidgrenMsg.Write(DateTaken);
            lidgrenMsg.Write(Width);
            lidgrenMsg.Write(Height);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Folder = lidgrenMsg.ReadString();
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
            return Folder.GetByteCount() + sizeof(long) + sizeof(ushort) * 2 + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
