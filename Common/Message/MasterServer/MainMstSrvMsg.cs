using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.MasterServer
{
    public class MainMstSrvMsg : MstSrvMsgBase<MsBaseMsgData>
    {        
        /// <inheritdoc />
        internal MainMstSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)MasterServerMessageSubType.RegisterServer] = MessageStore.GetMessageData<MsRegisterServerMsgData>(true),
            [(ushort)MasterServerMessageSubType.RequestServers] = MessageStore.GetMessageData<MsRequestServersMsgData>(true),
            [(ushort)MasterServerMessageSubType.ReplyServers] = MessageStore.GetMessageData<MsReplyServersMsgData>(true),
            [(ushort)MasterServerMessageSubType.Introduction] = MessageStore.GetMessageData<MsIntroductionMsgData>(true)
        };

        public override MasterServerMessageType MessageType => MasterServerMessageType.Main;
        protected override int DefaultChannel => 1;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}