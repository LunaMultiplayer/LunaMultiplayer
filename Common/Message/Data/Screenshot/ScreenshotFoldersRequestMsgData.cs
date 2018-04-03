using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotFoldersRequestMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotFoldersRequestMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.FoldersRequest;

        public override string ClassName { get; } = nameof(ScreenshotFoldersRequestMsgData);
    }
}