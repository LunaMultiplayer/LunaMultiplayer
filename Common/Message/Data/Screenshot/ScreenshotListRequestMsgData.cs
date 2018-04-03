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

        public int NumAlreadyOwnedPhotoIds;
        public long[] AlreadyOwnedPhotoIds = new long[0];

        public override string ClassName { get; } = nameof(ScreenshotListRequestMsgData);
        
        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FolderName);

            lidgrenMsg.Write(NumAlreadyOwnedPhotoIds);
            for (var i = 0; i < NumAlreadyOwnedPhotoIds; i++)
            {
                lidgrenMsg.Write(AlreadyOwnedPhotoIds[i]);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FolderName = lidgrenMsg.ReadString();

            NumAlreadyOwnedPhotoIds = lidgrenMsg.ReadInt32();
            if (AlreadyOwnedPhotoIds.Length < NumAlreadyOwnedPhotoIds)
                AlreadyOwnedPhotoIds = new long[NumAlreadyOwnedPhotoIds];

            for (var i = 0; i < NumAlreadyOwnedPhotoIds; i++)
            {
                AlreadyOwnedPhotoIds[i] = lidgrenMsg.ReadInt64();
            }
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + FolderName.GetByteCount() + sizeof(int) + sizeof(long) * NumAlreadyOwnedPhotoIds;
        }
    }
}