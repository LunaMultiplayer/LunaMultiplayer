using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data;
using LmpCommon.Message.Server.Base;

namespace LmpCommon.Message.Server
{
    public class ModSrvMsg : SrvMsgBase<ModMsgData>
    {
        /// <inheritdoc />
        internal ModSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ModSrvMsg);

        public override ServerMessageType MessageType => ServerMessageType.Mod;
        protected override int DefaultChannel => SendReliably() ? 15 : 0;
        public override NetDeliveryMethod NetDeliveryMethod => SendReliably() ?
            NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}