using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRespondMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryRespondMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.RespondFile;

        public string CraftOwner;
        public string RequestedName;
        public CraftType RequestedType;

        public int NumBytes;
        public byte[] CraftData = new byte[0];

        public override string ClassName { get; } = nameof(CraftLibraryRespondMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(CraftOwner);
            lidgrenMsg.Write(RequestedName);
            lidgrenMsg.Write((int)RequestedType);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(CraftData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            CraftOwner = lidgrenMsg.ReadString();
            RequestedName = lidgrenMsg.ReadString();
            RequestedType = (CraftType)lidgrenMsg.ReadInt32();

            NumBytes = lidgrenMsg.ReadInt32();

            if (CraftData.Length < NumBytes)
                CraftData = new byte[NumBytes];

            lidgrenMsg.ReadBytes(CraftData, 0, NumBytes);
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + CraftOwner.GetByteCount() + RequestedName.GetByteCount() + sizeof(CraftType)
                + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}