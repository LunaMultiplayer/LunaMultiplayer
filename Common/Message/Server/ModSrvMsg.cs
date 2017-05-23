using LunaCommon.Enums;
using LunaCommon.Message.Data;
using LunaCommon.Message.Server.Base;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class ModSrvMsg : SrvMsgBase<ModMsgData>
    {
        public override ServerMessageType MessageType => ServerMessageType.MOD;
        protected override int DefaultChannel => SendReliably() ? 15 : 0;
        public override NetDeliveryMethod NetDeliveryMethod => SendReliably() ?
            NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}