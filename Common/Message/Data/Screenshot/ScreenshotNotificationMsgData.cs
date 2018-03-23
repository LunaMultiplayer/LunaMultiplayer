using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
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
