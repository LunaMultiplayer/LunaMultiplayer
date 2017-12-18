using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.MasterServer
{
    public class MainMstSrvMsg : MstSrvMsgBase<MsBaseMsgData>
    {        
        /// <inheritdoc />
        internal MainMstSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(MainMstSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)MasterServerMessageSubType.RegisterServer] = typeof(MsRegisterServerMsgData),
            [(ushort)MasterServerMessageSubType.RequestServers] = typeof(MsRequestServersMsgData),
            [(ushort)MasterServerMessageSubType.ReplyServers] = typeof(MsReplyServersMsgData),
            [(ushort)MasterServerMessageSubType.Introduction] = typeof(MsIntroductionMsgData)
        };

        public override MasterServerMessageType MessageType => MasterServerMessageType.Main;
        protected override int DefaultChannel => 1;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}