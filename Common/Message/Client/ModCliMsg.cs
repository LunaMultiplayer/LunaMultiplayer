using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class ModCliMsg : CliMsgBase<ModMsgData>
    {
        public override ClientMessageType MessageType => ClientMessageType.MOD;
        protected override int DefaultChannel => SendReliably() ? 15 : 0;
        //Todo: Make UnreliableSequenced for non-reliable
        public override NetDeliveryMethod NetDeliveryMethod => SendReliably() ?
            NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.ReliableOrdered;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}