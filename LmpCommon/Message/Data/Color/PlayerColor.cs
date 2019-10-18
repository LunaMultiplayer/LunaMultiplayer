using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Color
{
    public class PlayerColor
    {
        public string PlayerName;
        public float[] Color = new float[3];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(Color[i]);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
            for (var i = 0; i < 3; i++)
                Color[i] = lidgrenMsg.ReadFloat();
        }

        public int GetByteCount()
        {
            return PlayerName.GetByteCount() + sizeof(float) * 3;
        }
    }
}
