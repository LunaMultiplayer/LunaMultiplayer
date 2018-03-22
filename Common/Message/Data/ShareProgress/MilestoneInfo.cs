using Lidgren.Network;
using LunaCommon.Message.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp ProgressNode objects (for the milestones).
    /// </summary>
    public class MilestoneInfo
    {
        public string Id;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Id = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return Id.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
