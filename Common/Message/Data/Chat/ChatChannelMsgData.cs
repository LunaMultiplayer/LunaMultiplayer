using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatChannelMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.ChannelMessage;

        private string ChannelPriv { get; set; }

        public string Channel
        {
            get => SendToAll ? "" : ChannelPriv;
            set => ChannelPriv = SendToAll ? "" : value;
        }

        public string Text { get; set; }

        public bool SendToAll { get; set; }
    }
}