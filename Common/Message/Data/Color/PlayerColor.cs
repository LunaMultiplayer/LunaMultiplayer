using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColor
    {
        public string PlayerName;
        public UnityEngine.Color Color;

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.WriteRgbColor(Color);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            PlayerName = lidgrenMsg.ReadString();
            Color = lidgrenMsg.ReadRgbColor();
        }

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + sizeof(byte) * 3;
        }
    }
}
