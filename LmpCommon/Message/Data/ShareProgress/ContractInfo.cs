using CachedQuickLz;
using Lidgren.Network;
using LmpCommon.Message.Base;
using System;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Wrapper for transmitting the ksp Contract objects.
    /// </summary>
    public class ContractInfo
    {
        public Guid ContractGuid;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public ContractInfo() { }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public ContractInfo(ContractInfo copyFrom)
        {
            ContractGuid = copyFrom.ContractGuid;
            NumBytes = copyFrom.NumBytes;
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            Array.Copy(copyFrom.Data, Data, NumBytes);
        }

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            GuidUtil.Serialize(ContractGuid, lidgrenMsg);

            CachedQlz.Compress(ref Data, ref NumBytes);
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
            CachedQlz.Decompress(ref Data, out NumBytes);
        }

        public int GetByteCount()
        {
            return GuidUtil.ByteSize + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
