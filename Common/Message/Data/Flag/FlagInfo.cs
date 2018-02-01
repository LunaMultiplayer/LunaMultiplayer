using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagInfo
    {
        public string Owner;
        public string FlagName;

        public int NumBytes;
        public byte[] FlagData = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Owner);
            lidgrenMsg.Write(FlagName);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(FlagData, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Owner = lidgrenMsg.ReadString();
            FlagName = lidgrenMsg.ReadString();
            NumBytes = lidgrenMsg.ReadInt32();
            
            if (FlagData.Length < NumBytes)
                FlagData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(FlagData, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return Owner.GetByteCount() + FlagName.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
