using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryDeleteMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryDeleteMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.DeleteFile;

        public CraftType CraftType;
        public string CraftName;

        public override string ClassName { get; } = nameof(CraftLibraryDeleteMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write((int)CraftType);
            lidgrenMsg.Write(CraftName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            CraftType = (CraftType)lidgrenMsg.ReadInt32();
            CraftName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(CraftType) + CraftName.GetByteCount();
        }
    }
}