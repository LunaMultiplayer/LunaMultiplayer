using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Chat;

namespace LmpCommon.Message.Client
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