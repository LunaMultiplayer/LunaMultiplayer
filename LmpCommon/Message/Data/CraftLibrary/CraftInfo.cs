using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftInfo
    {
        // Identifying information for the craft
        public string FolderName;
        public CraftType CraftType;
        public string CraftName;

        // Craft data - represents the .craft file
        public int CraftNumBytes;
        public byte[] CraftData = new byte[0];

        // Craft Info data - represents the .loadmeta file
        public int CraftInfoNumBytes;
        public byte[] CraftInfoData = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(CraftName);
            lidgrenMsg.Write((int)CraftType);

            // Write craft data
            Common.ThreadSafeCompress(this, ref CraftData, ref CraftNumBytes);

            lidgrenMsg.Write(CraftNumBytes);
            lidgrenMsg.Write(CraftData, 0, CraftNumBytes);

            // Write craft info data
            Common.ThreadSafeCompress(this, ref CraftInfoData, ref CraftInfoNumBytes);

            lidgrenMsg.Write(CraftInfoNumBytes);
            lidgrenMsg.Write(CraftInfoData, 0, CraftInfoNumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FolderName = lidgrenMsg.ReadString();
            CraftName = lidgrenMsg.ReadString();
            CraftType = (CraftType)lidgrenMsg.ReadInt32();

            // Read craft data
            CraftNumBytes = lidgrenMsg.ReadInt32();

            if (CraftData.Length < CraftNumBytes)
                CraftData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(CraftData, 0, CraftNumBytes);

            Common.ThreadSafeDecompress(this, ref CraftData, CraftNumBytes, out CraftNumBytes);

            // Read craft info data
            CraftInfoNumBytes = lidgrenMsg.ReadInt32();

            if (CraftInfoData.Length < CraftInfoNumBytes)
                CraftInfoData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(CraftInfoData, 0, CraftInfoNumBytes);

            Common.ThreadSafeDecompress(this, ref CraftInfoData, CraftInfoNumBytes, out CraftInfoNumBytes);
        }

        public int GetByteCount()
        {
            return FolderName.GetByteCount() + CraftName.GetByteCount() + sizeof(CraftType) + sizeof(int) + (sizeof(byte) * CraftNumBytes) + sizeof(int) + (sizeof(byte) * CraftInfoNumBytes);
        }
    }
}
