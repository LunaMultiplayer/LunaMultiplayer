using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Server.Base;

namespace LmpCommon.Message.Server
{
    public class ChatSrvMsg : SrvMsgBase<ChatMsgData>
    {
        /// <inheritdoc />
        internal ChatSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ChatSrvMsg);

        public override ServerMessageType MessageType => ServerMessageType.Chat;
        protected override int DefaultChannel => 3;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}