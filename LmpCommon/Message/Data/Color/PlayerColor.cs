using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Color
{
    public class PlayerColor
    {
        public string PlayerName;
        public UnityEngine.Color Color;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.WriteRgbColor(Color);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
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
