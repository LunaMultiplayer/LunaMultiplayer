using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotListReplyMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotListReplyMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.ListReply;

        public string FolderName;
        public int NumScreenshots;
        public ScreenshotInfo[] Screenshots = new ScreenshotInfo[0];

        public override string ClassName { get; } = nameof(ScreenshotListReplyMsgData);
        
        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(NumScreenshots);
            for (var i = 0; i < NumScreenshots; i++)
            {
                Screenshots[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FolderName = lidgrenMsg.ReadString();
            NumScreenshots = lidgrenMsg.ReadInt32();

            if (Screenshots.Length < NumScreenshots)
                Screenshots = new ScreenshotInfo[NumScreenshots];

            for (var i = 0; i < NumScreenshots; i++)
            {
                if (Screenshots[i] == null)
                    Screenshots[i] = new ScreenshotInfo();

                Screenshots[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < NumScreenshots; i++)
            {
                arraySize += Screenshots[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + FolderName.GetByteCount() + sizeof(int) + arraySize;
        }
    }
}