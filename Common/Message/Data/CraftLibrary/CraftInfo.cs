using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftInfo
    {
        public string FolderName;
        public string CraftName;
        public CraftType CraftType;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(CraftName);
            lidgrenMsg.Write((int)CraftType);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FolderName = lidgrenMsg.ReadString();
            CraftName = lidgrenMsg.ReadString();
            CraftType = (CraftType)lidgrenMsg.ReadInt32();
            
            NumBytes = lidgrenMsg.ReadInt32();

            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return FolderName.GetByteCount() + CraftName.GetByteCount() + sizeof(CraftType) + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
