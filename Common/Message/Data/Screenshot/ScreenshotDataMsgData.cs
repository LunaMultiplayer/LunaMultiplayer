using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotDataMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotDataMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.ScreenshotData;

        public int PhotoId;
        public int NumBytes;
        public byte[] Data = new byte[0];
        public bool IsSmall;

        public override string ClassName { get; } = nameof(ScreenshotDataMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PhotoId);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
            lidgrenMsg.Write(IsSmall);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PhotoId = lidgrenMsg.ReadInt32();
            NumBytes = lidgrenMsg.ReadInt32();

            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);

            IsSmall = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + sizeof(int) + sizeof(byte) * NumBytes + sizeof(bool);
        }
    }
}