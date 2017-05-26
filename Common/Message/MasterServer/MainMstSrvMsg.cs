using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.MasterServer
{
    public class MainMstSrvMsg : MstSrvMsgBase<MsBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)MasterServerMessageSubType.RegisterServer] = new MsRegisterServerMsgData(),
            [(ushort)MasterServerMessageSubType.RequestServers] = new MsRequestServersMsgData(),
            [(ushort)MasterServerMessageSubType.ReplyServers] = new MsReplyServersMsgData(),
            [(ushort)MasterServerMessageSubType.Introduction] = new MsIntroductionMsgData()
        };

        public override MasterServerMessageType MessageType => MasterServerMessageType.Main;
        protected override int DefaultChannel => 1;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}