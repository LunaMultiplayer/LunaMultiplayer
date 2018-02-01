using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryAddMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryAddMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.AddFile;

        public CraftType UploadType;
        public string UploadName;

        public override string ClassName { get; } = nameof(CraftLibraryAddMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write((int)UploadType);
            lidgrenMsg.Write(UploadName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            UploadType = (CraftType)lidgrenMsg.ReadInt32();
            UploadName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(CraftType) + UploadName.GetByteCount();
        }
    }
}