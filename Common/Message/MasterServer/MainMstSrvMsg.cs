using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer.Base;

namespace LunaCommon.Message.MasterServer
{
    public class MainMstSrvMsg : MstSrvMsgBase<MsBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)MasterServerMessageSubType.REGISTER_SERVER] = new MsRegisterServerMsgData(),
            [(ushort)MasterServerMessageSubType.REQUEST_SERVERS] = new MsRequestServersMsgData(),
            [(ushort)MasterServerMessageSubType.REPLY_SERVERS] = new MsReplyServersMsgData(),
            [(ushort)MasterServerMessageSubType.INTRODUCTION] = new MsIntroductionMsgData(),
        };

        public override MasterServerMessageType MessageType => MasterServerMessageType.MAIN;
        protected override int DefaultChannel => 1;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}