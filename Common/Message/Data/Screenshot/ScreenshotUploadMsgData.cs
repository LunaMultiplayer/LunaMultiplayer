using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotUploadMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotUploadMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.Upload;

        public int NumBytes;
        public byte[] Data = new byte[0];

        public override string ClassName { get; } = nameof(ScreenshotUploadMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            
            NumBytes = lidgrenMsg.ReadInt32();

            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}