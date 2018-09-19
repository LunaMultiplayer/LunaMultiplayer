using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryListRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.ListRequest;

        public string FolderName;

        public override string ClassName { get; } = nameof(CraftLibraryListRequestMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FolderName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FolderName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + FolderName.GetByteCount();
        }
    }
}
