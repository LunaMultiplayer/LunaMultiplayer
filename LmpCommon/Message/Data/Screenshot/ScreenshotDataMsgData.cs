using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Screenshot
{
    public class ScreenshotDataMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotDataMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.ScreenshotData;

        public ScreenshotInfo Screenshot = new ScreenshotInfo();

        public override string ClassName { get; } = nameof(ScreenshotDataMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            Screenshot.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            Screenshot.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Screenshot.GetByteCount();
        }
    }
}