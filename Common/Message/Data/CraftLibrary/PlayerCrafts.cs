using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class PlayerCrafts
    {
        public string PlayerName;
        public CraftListInfo Crafts = new CraftListInfo();

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            lidgrenMsg.Write(PlayerName);
            Crafts.Serialize(lidgrenMsg, compressData);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            PlayerName = lidgrenMsg.ReadString();
            Crafts.Deserialize(lidgrenMsg, dataCompressed);
        }
        
        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + Crafts.GetByteCount();
        }
    }
}
