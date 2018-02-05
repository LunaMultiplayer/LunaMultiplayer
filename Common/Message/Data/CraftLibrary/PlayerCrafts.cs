using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class PlayerCrafts
    {
        public string PlayerName;
        public CraftListInfo Crafts = new CraftListInfo();

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            Crafts.Serialize(lidgrenMsg);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
            Crafts.Deserialize(lidgrenMsg);
        }
        
        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + Crafts.GetByteCount();
        }
    }
}
