using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data;
using LunaCommon.Message.Server.Base;

namespace LunaCommon.Message.Server
{
    public class ModSrvMsg : SrvMsgBase<ModMsgData>
    {
        /// <inheritdoc />
        internal ModSrvMsg() { }

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