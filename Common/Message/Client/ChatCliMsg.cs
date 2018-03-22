using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Chat;

namespace LunaCommon.Message.Client
{
    public class ChatCliMsg : CliMsgBase<ChatMsgData>
    {        
        /// <inheritdoc />
        internal ChatCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ChatCliMsg);

        public override ClientMessageType MessageType => ClientMessageType.Chat;

        protected override int DefaultChannel => 3;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}