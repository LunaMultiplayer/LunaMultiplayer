using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.RequestFile;

        public string CraftOwner;
        public CraftType RequestedType;
        public string RequestedName;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(CraftOwner);
            lidgrenMsg.Write((int)RequestedType);
            lidgrenMsg.Write(RequestedName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            CraftOwner = lidgrenMsg.ReadString();
            RequestedType = (CraftType)lidgrenMsg.ReadInt32();
            RequestedName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + CraftOwner.GetByteCount() +sizeof(CraftType) + RequestedName.GetByteCount();
        }
    }
}