using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotDownloadRequestMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotDownloadRequestMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.DownloadRequest;

        public string FolderName;
        public long PhotoId;

        public override string ClassName { get; } = nameof(ScreenshotDownloadRequestMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(PhotoId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FolderName = lidgrenMsg.ReadString();
            PhotoId = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + FolderName.GetByteCount() + sizeof(long);
        }
    }
}