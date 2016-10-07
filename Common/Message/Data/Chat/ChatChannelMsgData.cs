using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatChannelMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.CHANNEL_MESSAGE;

        private string ChannelPriv { get; set; }

        public string Channel
        {
            get { return SendToAll ? "" : ChannelPriv; }
            set { ChannelPriv = SendToAll ? "" : value; }
        }

        public string Text { get; set; }

        public bool SendToAll { get; set; }
    }
}