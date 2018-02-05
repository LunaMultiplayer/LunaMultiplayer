using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusReplyMsgData: PlayerStatusBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerStatusReplyMsgData() { }
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Reply;

        public int PlayerStatusCount;
        public PlayerStatusInfo[] PlayerStatus = new PlayerStatusInfo[0];

        public override string ClassName { get; } = nameof(PlayerStatusReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PlayerStatusCount);
            for (var i = 0; i < PlayerStatusCount; i++)
            {
                PlayerStatus[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerStatusCount = lidgrenMsg.ReadInt32();
            if (PlayerStatus.Length < PlayerStatusCount)
                PlayerStatus = new PlayerStatusInfo[PlayerStatusCount];

            for (var i = 0; i < PlayerStatusCount; i++)
            {
                if (PlayerStatus[i] == null)
                    PlayerStatus[i] = new PlayerStatusInfo();

                PlayerStatus[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerStatusCount; i++)
            {
                arraySize += PlayerStatus[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
