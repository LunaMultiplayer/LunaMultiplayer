using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatPrivateMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatPrivateMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.PrivateMessage;

        public string To;
        public string Text;

        public override string ClassName { get; } = nameof(ChatPrivateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(To);
            lidgrenMsg.Write(Text);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            To = lidgrenMsg.ReadString();
            Text = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + To.GetByteCount() + Text.GetByteCount();
        }
    }
}