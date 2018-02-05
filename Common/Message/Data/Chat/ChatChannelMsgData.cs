using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatChannelMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatChannelMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ChannelMessage;

        public bool SendToAll;
        public string Channel;
        public string Text;

        public override string ClassName { get; } = nameof(ChatChannelMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(SendToAll);
            if (SendToAll)
            {
                Channel = string.Empty;
            }
            lidgrenMsg.WritePadBits();

            lidgrenMsg.Write(Channel);
            lidgrenMsg.Write(Text);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            SendToAll = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();

            Channel = lidgrenMsg.ReadString();
            Text = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            //We use sizeof(byte) instead of sizeof(bool) because we use the WritePadBits()
            return base.InternalGetMessageSize() + sizeof(byte) + Channel.GetByteCount() + Text.GetByteCount();
        }
    }
}