using System;
using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class WarpSrvMsg : SrvMsgBase<WarpBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)WarpMessageType.SUBSPACES_REPLY] = new WarpSubspacesReplyMsgData(),
            [(ushort)WarpMessageType.NEW_SUBSPACE] = new WarpNewSubspaceMsgData(),
            [(ushort)WarpMessageType.CHANGE_SUBSPACE] = new WarpChangeSubspaceMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.WARP;
        protected override int DefaultChannel => 13;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}