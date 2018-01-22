using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Warp
{
    public class SubspaceInfo
    {
        public int SubspaceKey;
        public double SubspaceTime;

        public int PlayerCount;
        public string[] Players = new string[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            lidgrenMsg.Write(SubspaceKey);
            lidgrenMsg.Write(SubspaceTime);

            lidgrenMsg.Write(PlayerCount);
            for (var i = 0; i < PlayerCount; i++)
                lidgrenMsg.Write(Players[i]);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            SubspaceKey = lidgrenMsg.ReadInt32();
            SubspaceTime = lidgrenMsg.ReadDouble();

            PlayerCount = lidgrenMsg.ReadInt32();
            if (Players.Length < PlayerCount )
                Players = new string[PlayerCount];

            for (var i = 0; i < PlayerCount; i++)
                Players[i] = lidgrenMsg.ReadString();
        }
        
        public int GetByteCount(bool dataCompressed)
        {
            return sizeof(int) + sizeof(double) + sizeof(int) + Players.GetByteCount(PlayerCount);
        }
    }
}
