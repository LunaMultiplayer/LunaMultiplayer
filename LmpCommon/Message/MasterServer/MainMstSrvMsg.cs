using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Message.MasterServer.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.MasterServer
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