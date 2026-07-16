using Lidgren.Network;
using LmpCommon.Message.Base;
using System;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp Strategy objects (for the career strategies).
    /// </summary>
    public class StrategyInfo
    {
        public string Name;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public StrategyInfo()
        {

        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copyFrom"></param>
        public StrategyInfo(StrategyInfo copyFrom)
        {
            Name = copyFrom.Name;
            NumBytes = copyFrom.NumBytes;
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            Array.Copy(copyFrom.Data, Data, NumBytes);
        }

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Name);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Name = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return Name.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
