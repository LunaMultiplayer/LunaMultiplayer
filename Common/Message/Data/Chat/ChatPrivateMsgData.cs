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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(To);
            lidgrenMsg.Write(Text);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            To = lidgrenMsg.ReadString();
            Text = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + To.GetByteCount() + Text.GetByteCount();
        }
    }
}