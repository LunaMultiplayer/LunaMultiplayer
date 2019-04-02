using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListReplyMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryListReplyMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.ListReply;

        public string FolderName;
        public int PlayerCraftsCount;
        public CraftBasicInfo[] PlayerCrafts = new CraftBasicInfo[0];

        public override string ClassName { get; } = nameof(CraftLibraryListReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FolderName);
            lidgrenMsg.Write(PlayerCraftsCount);
            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                PlayerCrafts[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FolderName = lidgrenMsg.ReadString();
            PlayerCraftsCount = lidgrenMsg.ReadInt32();

            if (PlayerCrafts.Length < PlayerCraftsCount)
                PlayerCrafts = new CraftBasicInfo[PlayerCraftsCount];

            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                if (PlayerCrafts[i] == null)
                    PlayerCrafts[i] = new CraftBasicInfo();

                PlayerCrafts[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                arraySize += PlayerCrafts[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + FolderName.GetByteCount() + sizeof(int) + arraySize;
        }
    }
}
