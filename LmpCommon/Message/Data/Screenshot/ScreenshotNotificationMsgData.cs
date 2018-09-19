using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Screenshot
{
    public class ScreenshotNotificationMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotNotificationMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.Notification;

        public string FolderName;

        public override string ClassName { get; } = nameof(ScreenshotNotificationMsgData);

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
