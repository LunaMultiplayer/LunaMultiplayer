using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data;

namespace LmpCommon.Message.Client
{
    public class ModCliMsg : CliMsgBase<ModMsgData>
    {
        /// <inheritdoc />
        internal ModCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ModCliMsg);

        public override ClientMessageType MessageType => ClientMessageType.Mod;
        protected override int DefaultChannel => SendReliably() ? 15 : 0;
        public override NetDeliveryMethod NetDeliveryMethod => SendReliably() ?
            NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}