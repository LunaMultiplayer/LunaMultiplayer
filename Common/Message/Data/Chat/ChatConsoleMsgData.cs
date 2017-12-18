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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(Message);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Message = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Message.GetByteCount();
        }
    }
}