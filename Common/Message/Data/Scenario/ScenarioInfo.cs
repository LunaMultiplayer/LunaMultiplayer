using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioInfo
    {
        public string Module;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            lidgrenMsg.Write(Module);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            Module = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount(bool dataCompressed)
        {
            return Module.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
