using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatConsoleMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatConsoleMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ConsoleMessage;

        public string Message;

        public override string ClassName { get; } = nameof(ChatConsoleMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Message);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Message = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Message.GetByteCount();
        }
    }
}