using Lidgren.Network;
using LunaCommon.Message.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp Contract objects.
    /// </summary>
    public class ContractInfo
    {
        public Guid ContractGuid;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            GuidUtil.Serialize(ContractGuid, lidgrenMsg);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            ContractGuid = GuidUtil.Deserialize(lidgrenMsg);

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        public int GetByteCount()
        {
            return GuidUtil.GetByteSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
