using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Scenario
{
    public class ScenarioInfo
    {
        public string Module;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Module);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Module = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return Module.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
