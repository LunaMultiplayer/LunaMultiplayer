using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotListRequestMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotListRequestMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.ListRequest;

        public string FolderName;

        public override string ClassName { get; } = nameof(ScreenshotListRequestMsgData);
        
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