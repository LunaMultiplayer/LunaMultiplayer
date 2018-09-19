using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryNotificationMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryNotificationMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.Notification;

        public string FolderName;

        public override string ClassName { get; } = nameof(CraftLibraryNotificationMsgData);

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
