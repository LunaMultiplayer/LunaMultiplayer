using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatListReplyMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatListReplyMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ListReply;

        public PlayerChatChannels[] PlayerChannels = new PlayerChatChannels[0];
        public int PlayerChannelsCount;

        public override string ClassName { get; } = nameof(ChatListReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerChannelsCount);
            for (var i = 0; i < PlayerChannelsCount; i++)
            {
                PlayerChannels[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerChannelsCount = lidgrenMsg.ReadInt32();
            PlayerChannels = ArrayPool<PlayerChatChannels>.Claim(PlayerChannelsCount);
            for (var i = 0; i < PlayerChannelsCount; i++)
            {
                if (PlayerChannels[i] == null)
                    PlayerChannels[i] = new PlayerChatChannels();

                PlayerChannels[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < PlayerChannelsCount; i++)
            {
                PlayerChannels[i].Recycle();
            }
            ArrayPool<PlayerChatChannels>.Release(ref PlayerChannels);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerChannelsCount; i++)
            {
                arraySize += PlayerChannels[i].GetByteCount();
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}