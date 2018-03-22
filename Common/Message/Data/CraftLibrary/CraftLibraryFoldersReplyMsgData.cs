using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryFoldersReplyMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryFoldersReplyMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.FoldersReply;

        public int NumFolders;
        public string[] Folders = new string[0];

        public override string ClassName { get; } = nameof(CraftLibraryFoldersReplyMsgData);

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
