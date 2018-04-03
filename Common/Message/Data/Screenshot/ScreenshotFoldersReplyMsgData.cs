using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Screenshot
{
    public class ScreenshotFoldersReplyMsgData : ScreenshotBaseMsgData
    {
        /// <inheritdoc />
        internal ScreenshotFoldersReplyMsgData() { }
        public override ScreenshotMessageType ScreenshotMessageType => ScreenshotMessageType.FoldersReply;

        public int NumFolders;
        public string[] Folders = new string[0];

        public override string ClassName { get; } = nameof(ScreenshotFoldersReplyMsgData);
        
        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(NumFolders);
            for (var i = 0; i < NumFolders; i++)
            {
                lidgrenMsg.Write(Folders[i]);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            NumFolders = lidgrenMsg.ReadInt32();

            if (Folders.Length < NumFolders)
                Folders = new string[NumFolders];

            for (var i = 0; i < NumFolders; i++)
            {
                Folders[i] = lidgrenMsg.ReadString();
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < NumFolders; i++)
            {
                arraySize += Folders[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}