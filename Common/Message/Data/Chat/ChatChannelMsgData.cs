using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatChannelMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatChannelMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ChannelMessage;

        private string ChannelPriv { get; set; }

        public string Channel
        {
            get => SendToAll ? string.Empty : ChannelPriv;
            set => ChannelPriv = SendToAll ? string.Empty : value;
        }

        public string Text { get; set; }

        public bool SendToAll { get; set; }
    }
}