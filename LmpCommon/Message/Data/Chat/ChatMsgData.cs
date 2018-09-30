using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Chat
{
    public class ChatMsgData : MessageData
    {
        /// <inheritdoc />
        internal ChatMsgData() { }
        public override bool CompressCondition => false;

        public string From;
        public string Text;
        public bool Relay;

        public override string ClassName { get; } = nameof(ChatMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(From);
            lidgrenMsg.Write(Text);
            lidgrenMsg.Write(Relay);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            From = lidgrenMsg.ReadString();
            Text = lidgrenMsg.ReadString();
            Relay = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {
            return From.GetByteCount() + Text.GetByteCount() + sizeof(bool);
        }
    }
}