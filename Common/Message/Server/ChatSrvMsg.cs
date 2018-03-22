using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server.Base;

namespace LunaCommon.Message.Server
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