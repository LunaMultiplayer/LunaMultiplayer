using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftBasicInfo
    {
        public string FolderName;
        public string CraftName;
        public CraftType CraftType;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(CraftName);
            lidgrenMsg.Write((int)CraftType);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FolderName = lidgrenMsg.ReadString();
            CraftName = lidgrenMsg.ReadString();
            CraftType = (CraftType)lidgrenMsg.ReadInt32();
        }

        public int GetByteCount()
        {
            return FolderName.GetByteCount() + CraftName.GetByteCount() + sizeof(CraftType);
        }
    }
}
